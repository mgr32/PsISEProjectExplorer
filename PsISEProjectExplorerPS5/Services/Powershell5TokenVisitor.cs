using System;
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

        public Powershell5TokenVisitor(
            Action<string, IScriptExtent> functionAction,
            Action<string, IScriptExtent> configurationAction,
            Func<string, IScriptExtent, object> classAction,
            Action<string, IScriptExtent, object> classPropertyAction,
            Action<string, IScriptExtent, object> classConstructorAction,
            Action<string, IScriptExtent, object> classMethodAction
            )
        {
            this.functionAction = functionAction;
            this.configurationAction = configurationAction;
            this.classAction = classAction;
            this.classPropertyAction = classPropertyAction;
            this.classConstructorAction = classConstructorAction;
            this.classMethodAction = classMethodAction;

        }


        public override AstVisitAction VisitFunctionDefinition(FunctionDefinitionAst functionDefinitionAst)
        {
            this.functionAction(functionDefinitionAst.Name, functionDefinitionAst.Extent);
            return AstVisitAction.Continue;
        }
       
        public override AstVisitAction VisitConfigurationDefinition(ConfigurationDefinitionAst configurationDefinitionAst)
        {
            this.configurationAction(configurationDefinitionAst.InstanceName.ToString(), configurationDefinitionAst.Extent);
            return AstVisitAction.Continue;
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
            return AstVisitAction.Continue;
        }
        
    }
}
