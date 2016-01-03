using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation.Language;

namespace PsISEProjectExplorer.Services
{
    // This is required to be in different assembly because AstVisitor2 has been introduced in PS5
    // in the same assembly as the one used by PS3 (without changing the assembly version).
    public class Powershell5TokenVisitor : AstVisitor2
    {
        private Func<string, IScriptExtent, int, object, object> FunctionAction;

        private Func<string, IScriptExtent, int, object, object> ConfigurationAction;

        private Func<string, IScriptExtent, int, object, object> ClassAction;

        private Func<string, IScriptExtent, int, object, object> ClassPropertyAction;

        private Func<string, IScriptExtent, int, object, object> ClassConstructorAction;

        private Func<string, IScriptExtent, int, object, object> ClassMethodAction;

        private Func<CommandAst, string> DslNameGiver;

        private Func<string, string, IScriptExtent, int, object, object> DslAction;

        private Stack<object> ParentObjectStack = new Stack<object>();

        public Powershell5TokenVisitor(
            Func<string, IScriptExtent, int, object, object> functionAction,
            Func<string, IScriptExtent, int, object, object> configurationAction,
            Func<string, IScriptExtent, int, object, object> classAction,
            Func<string, IScriptExtent, int, object, object> classPropertyAction,
            Func<string, IScriptExtent, int, object, object> classConstructorAction,
            Func<string, IScriptExtent, int, object, object> classMethodAction,
            Func<CommandAst, string> dslNameGiver,
            Func<string, string, IScriptExtent, int, object, object> dslAction
            )
        {
            this.FunctionAction = functionAction;
            this.ConfigurationAction = configurationAction;
            this.ClassAction = classAction;
            this.ClassPropertyAction = classPropertyAction;
            this.ClassConstructorAction = classConstructorAction;
            this.ClassMethodAction = classMethodAction;
            this.DslNameGiver = dslNameGiver;
            this.DslAction = dslAction;
        }

        public override AstVisitAction VisitCommand(CommandAst commandAst)
        {
            string dslInstanceName = this.DslNameGiver(commandAst);
            if (dslInstanceName == null)
            {
                return AstVisitAction.Continue;
            }

            var commandElements = commandAst.CommandElements;
            string dslTypeName = ((StringConstantExpressionAst)commandElements[0]).Value;
            object newParentObject = this.DslAction(dslTypeName, dslInstanceName, commandAst.Extent, this.GetCurrentNestingLevel(), this.GetCurrentParentObject());
            Ast body = commandElements[commandElements.Count - 1];
            this.VisitChildren(commandAst, newParentObject);
            return AstVisitAction.SkipChildren;
        }


        public override AstVisitAction VisitFunctionDefinition(FunctionDefinitionAst functionDefinitionAst)
        {
            object newParentObject = this.FunctionAction(functionDefinitionAst.Name, functionDefinitionAst.Extent, this.GetCurrentNestingLevel(), this.GetCurrentParentObject());
            this.VisitChildren(functionDefinitionAst.Body, newParentObject);
            return AstVisitAction.SkipChildren;
        }
       
        public override AstVisitAction VisitConfigurationDefinition(ConfigurationDefinitionAst configurationDefinitionAst)
        {
            object newParentObject = this.ConfigurationAction(configurationDefinitionAst.InstanceName.ToString(), configurationDefinitionAst.Extent, this.GetCurrentNestingLevel(), this.GetCurrentParentObject());
            this.VisitChildren(configurationDefinitionAst.Body, newParentObject);
            return AstVisitAction.SkipChildren;
        }

        public override AstVisitAction VisitTypeDefinition(TypeDefinitionAst typeDefinitionAst)
        {

            object item = this.ClassAction(typeDefinitionAst.Name, typeDefinitionAst.Extent, this.GetCurrentNestingLevel(), this.GetCurrentParentObject());
            foreach (MemberAst member in typeDefinitionAst.Members)
            {
                if (member is PropertyMemberAst)
                {
                   this.ClassPropertyAction(member.Name, member.Extent, this.GetCurrentNestingLevel() + 1, item);
                } 
                else if (member is FunctionMemberAst)
                {
                    FunctionMemberAst functionMember = (FunctionMemberAst)member;
                    object newParentObject;
                    if (functionMember.IsConstructor)
                    {
                        newParentObject = this.ClassConstructorAction(member.Name, member.Extent, this.GetCurrentNestingLevel() + 1, item);                        
                    }
                    else
                    {
                        newParentObject = this.ClassMethodAction(member.Name, member.Extent, this.GetCurrentNestingLevel() + 1, item);
                    }
                    this.VisitChildren(functionMember.Body, newParentObject);
                }
            }
            return AstVisitAction.SkipChildren;
        }

        public void VisitTokens(Ast ast)
        {
            ast.Visit(this);
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

    }
}
