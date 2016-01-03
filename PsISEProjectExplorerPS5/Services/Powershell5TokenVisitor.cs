using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation.Language;

namespace PsISEProjectExplorer.Services
{
    public class Powershell5TokenVisitor : AstVisitor2
    {
        private Action<string, IScriptExtent> functionAction;

        private Action<string, IScriptExtent> configurationAction;

        private Func<string, IScriptExtent, object> classAction;

        private Action<string, IScriptExtent, object> classPropertyAction;

        private Action<string, IScriptExtent, object> classConstructorAction;

        private Action<string, IScriptExtent, object> classMethodAction;

        private Func<string, string, int, IScriptExtent, object, object> dslAction;

        private Stack<object> parentObjectStack = new Stack<object>();

        public Powershell5TokenVisitor(
            Action<string, IScriptExtent> functionAction,
            Action<string, IScriptExtent> configurationAction,
            Func<string, IScriptExtent, object> classAction,
            Action<string, IScriptExtent, object> classPropertyAction,
            Action<string, IScriptExtent, object> classConstructorAction,
            Action<string, IScriptExtent, object> classMethodAction,
            Func<string, string, int, IScriptExtent, object, object> dslAction
            )
        {
            this.functionAction = functionAction;
            this.configurationAction = configurationAction;
            this.classAction = classAction;
            this.classPropertyAction = classPropertyAction;
            this.classConstructorAction = classConstructorAction;
            this.classMethodAction = classMethodAction;
            this.dslAction = dslAction;
        }

        public override AstVisitAction VisitCommand(CommandAst commandAst)
        {
            var commandElements = commandAst.CommandElements;
            if (commandElements == null || commandElements.Count < 2)
            {
                return AstVisitAction.Continue;
            }
            // in order to be possibly a DSL expression, first element must be StringConstant, second must not be =, last must be ScriptBlockExpression, and last but 1 must not be CommandParameter
            if (!(commandElements[0] is StringConstantExpressionAst) || 
                ((commandElements[1] is StringConstantExpressionAst && ((StringConstantExpressionAst)commandElements[1]).Value == "=")) ||
                !(commandElements[commandElements.Count-1] is ScriptBlockExpressionAst) ||
                commandElements[commandElements.Count-2] is CommandParameterAst)
            {
                return AstVisitAction.Continue;
            }
            
            // additionally, parent must not be a Pipeline that has more than 1 element 
            if (commandAst.Parent is PipelineAst && ((PipelineAst)commandAst.Parent).PipelineElements.Count > 1)
            {
                return AstVisitAction.Continue; 
            }

            
            string dslTypeName = ((StringConstantExpressionAst)commandElements[0]).Value;
            string dslInstanceName = GetDslInstanceName(commandElements);
            object currentParentObject = parentObjectStack.Count == 0 ? null : parentObjectStack.Peek();
            object newParentObject = this.dslAction(dslTypeName, dslInstanceName, parentObjectStack.Count, commandAst.Extent, currentParentObject);
            parentObjectStack.Push(newParentObject);
            commandElements[commandElements.Count - 1].Visit(this);
            parentObjectStack.Pop();
            return AstVisitAction.SkipChildren;
        }

        private string GetDslInstanceName(IEnumerable<CommandElementAst> commandElements)
        {
            // try to guess dsl instance name - first string constant that is not named parameter value (or is value of 'name' parameter)
            bool lastElementIsUnknownParameter = false;
            int num = 0;
            
            foreach (CommandElementAst elementAst in commandElements)
            {
                if (num++ == 0)
                {
                    continue;
                }
                if (elementAst is CommandParameterAst)
                {
                    lastElementIsUnknownParameter = ((CommandParameterAst)elementAst).ParameterName.ToLowerInvariant() != "name";
                    continue;
                }
                if (elementAst is StringConstantExpressionAst && !lastElementIsUnknownParameter)
                {
                    return ((StringConstantExpressionAst)elementAst).Value;
                }
                if (elementAst is ExpandableStringExpressionAst && !lastElementIsUnknownParameter)
                {
                    return ((ExpandableStringExpressionAst)elementAst).Value;
                }
                lastElementIsUnknownParameter = false;  
            }
            return string.Empty;
        }


        public override AstVisitAction VisitFunctionDefinition(FunctionDefinitionAst functionDefinitionAst)
        {
            this.functionAction(functionDefinitionAst.Name, functionDefinitionAst.Extent);
            return base.VisitFunctionDefinition(functionDefinitionAst);
        }
       
        public override AstVisitAction VisitConfigurationDefinition(ConfigurationDefinitionAst configurationDefinitionAst)
        {
            this.configurationAction(configurationDefinitionAst.InstanceName.ToString(), configurationDefinitionAst.Extent);
            return base.VisitConfigurationDefinition(configurationDefinitionAst);
        }

        public override AstVisitAction VisitTypeDefinition(TypeDefinitionAst typeDefinitionAst)
        {

            object item = this.classAction(typeDefinitionAst.Name, typeDefinitionAst.Extent);
            foreach (MemberAst member in typeDefinitionAst.Members)
            {
                if (member is PropertyMemberAst)
                {
                    this.classPropertyAction(member.Name, member.Extent, item);
                } 
                else if (member is FunctionMemberAst)
                {
                    FunctionMemberAst functionMember = (FunctionMemberAst)member;
                    if (functionMember.IsConstructor)
                    {
                        this.classConstructorAction(member.Name, member.Extent, item);
                    }
                    else
                    {
                        this.classMethodAction(member.Name, member.Extent, item);
                    }
                }
            }
            return base.VisitTypeDefinition(typeDefinitionAst);
        }
        
    }
}
