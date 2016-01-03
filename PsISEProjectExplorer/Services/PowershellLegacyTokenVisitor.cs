using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation.Language;

namespace PsISEProjectExplorer.Services
{
    public class PowershellLegacyTokenVisitor : AstVisitor
    {
        private Func<string, IScriptExtent, int, object, object> FunctionAction;

        private Func<string, IScriptExtent, int, object, object> ConfigurationAction;

        private Func<ReadOnlyCollection<CommandElementAst>, Ast, string> DslNameGiver;

        private Func<string, string, IScriptExtent, int, object, object> DslAction;

        private Stack<object> ParentObjectStack = new Stack<object>();

        public PowershellLegacyTokenVisitor(
            Func<string, IScriptExtent, int, object, object> functionAction,
            Func<string, IScriptExtent, int, object, object> configurationAction,
            Func<ReadOnlyCollection<CommandElementAst>, Ast, string> dslNameGiver,
            Func<string, string, IScriptExtent, int, object, object> dslAction
            )
        {
            this.FunctionAction = functionAction;
            this.ConfigurationAction = configurationAction;
            this.DslNameGiver = dslNameGiver;
            this.DslAction = dslAction;
        }

        public override AstVisitAction VisitCommand(CommandAst commandAst)
        {
            // in legacy visitor we need to check for Configuration ourselves (in PS5 this is done in VisitConfigurationDefinition)
            CommandParameterAst configurationAst = this.GetConfigurationAst(commandAst);
            object newParentObject;
            if (configurationAst != null)
            {
                string name = configurationAst.Argument.ToString();
                newParentObject = this.ConfigurationAction(name, commandAst.Extent, this.GetCurrentNestingLevel(), this.GetCurrentParentObject());
                Ast body = commandAst.Find(ast => ast is CommandParameterAst && "body".Equals(((CommandParameterAst)ast).ParameterName.ToLowerInvariant()), false);
                // TODO: this will ignore "Node" children because last command element is CommmandParameterAst
                this.VisitChildren(body, newParentObject);
                return AstVisitAction.SkipChildren;
            }

            var commandElements = commandAst.CommandElements;
            string dslInstanceName = this.DslNameGiver(commandElements, commandAst.Parent);
            if (dslInstanceName == null)
            {
                return AstVisitAction.Continue;
            }

            string dslTypeName = ((StringConstantExpressionAst)commandElements[0]).Value;
            newParentObject = this.DslAction(dslTypeName, dslInstanceName, commandAst.Extent, this.GetCurrentNestingLevel(), this.GetCurrentParentObject());
            this.VisitChildren(commandElements[commandElements.Count - 1], newParentObject);
            return AstVisitAction.SkipChildren;
        }

        public override AstVisitAction VisitFunctionDefinition(FunctionDefinitionAst functionDefinitionAst)
        {
            object newParentObject = this.FunctionAction(functionDefinitionAst.Name, functionDefinitionAst.Extent, this.GetCurrentNestingLevel(), this.GetCurrentParentObject());
            this.VisitChildren(functionDefinitionAst.Body, newParentObject);
            return AstVisitAction.SkipChildren;
        }

        private CommandParameterAst GetConfigurationAst(CommandAst commandAst)
        {
            var commandElements = commandAst.CommandElements;
            if (commandElements == null || commandElements.Count < 2)
            {
                return null;
            }

            if ((commandElements[0] is StringConstantExpressionAst) && "PSDesiredStateConfiguration\\Configuration".Equals(((StringConstantExpressionAst)commandElements[0]).Value))
            {
                Ast nameAst = commandAst.Find(ast => ast is CommandParameterAst && "name".Equals(((CommandParameterAst)ast).ParameterName.ToLowerInvariant()), false);
                return (CommandParameterAst)nameAst;
            }
            return null;
        }

        private object GetCurrentParentObject()
        {
            return this.ParentObjectStack.Count == 0 ? null : this.ParentObjectStack.Peek();
        }

        private int GetCurrentNestingLevel()
        {
            return this.ParentObjectStack.Count;
        }

        private void VisitChildren(Ast ast, object newParentObject)
        {
            this.ParentObjectStack.Push(newParentObject);
            ast.Visit(this);
            this.ParentObjectStack.Pop();
        }

        public void visitTokens(Ast ast)
        {
            ast.Visit(this);
        }

    }
}
