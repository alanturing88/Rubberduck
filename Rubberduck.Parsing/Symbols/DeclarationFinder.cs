using NLog;
using Rubberduck.Parsing.Annotations;
using Rubberduck.VBEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime;

namespace Rubberduck.Parsing.Symbols
{
    public class DeclarationFinder
    {
        private readonly IDictionary<QualifiedModuleName, CommentNode[]> _comments;
        private readonly IDictionary<QualifiedModuleName, IAnnotation[]> _annotations;
        private readonly IDictionary<QualifiedMemberName, IList<Declaration>> _undeclared;

        private readonly IReadOnlyList<Declaration> _declarations;
        private readonly IDictionary<string, Declaration[]> _declarationsByName;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public DeclarationFinder(
            IReadOnlyList<Declaration> declarations,
            IEnumerable<CommentNode> comments,
            IEnumerable<IAnnotation> annotations)
        {
            _comments = comments.GroupBy(node => node.QualifiedSelection.QualifiedName)
                .ToDictionary(grouping => grouping.Key, grouping => grouping.ToArray());
            _annotations = annotations.GroupBy(node => node.QualifiedSelection.QualifiedName)
                .ToDictionary(grouping => grouping.Key, grouping => grouping.ToArray());
            _declarations = declarations;
            _declarationsByName = declarations.GroupBy(declaration => new
            {
                IdentifierName = declaration.IdentifierName.ToLowerInvariant()
            })
            .ToDictionary(grouping => grouping.Key.IdentifierName, grouping => grouping.ToArray());

            _undeclared = new Dictionary<QualifiedMemberName, IList<Declaration>>();
        }

        public IEnumerable<Declaration> Undeclared
        {
            get { return _undeclared.SelectMany(e => e.Value); }
        }

        public IEnumerable<Declaration> FindDeclarationsWithNonBaseAsType()
        {
            return _declarations
                .Where(d =>
                !string.IsNullOrWhiteSpace(d.AsTypeName)
                && !d.AsTypeIsBaseType
                && d.DeclarationType != DeclarationType.Project
                && d.DeclarationType != DeclarationType.ProceduralModule).ToList();
        }

        public IEnumerable<Declaration> FindClasses()
        {
            return _declarations.Where(d => d.DeclarationType == DeclarationType.ClassModule).ToList();
        }

        public IEnumerable<Declaration> FindProjects()
        {
            return _declarations.Where(d => d.DeclarationType == DeclarationType.Project).ToList();
        }

        public Declaration FindParameter(Declaration procedure, string parameterName)
        {
            var matches = MatchName(parameterName);
            return matches.FirstOrDefault(m => procedure.Equals(m.ParentDeclaration) && m.DeclarationType == DeclarationType.Parameter);
        }

        public IEnumerable<IAnnotation> FindAnnotations(QualifiedModuleName module)
        {
            IAnnotation[] result;
            if (_annotations.TryGetValue(module, out result))
            {
                return result;
            }
            return new List<IAnnotation>();
        }

        public bool IsMatch(string declarationName, string potentialMatchName)
        {
            return string.Equals(declarationName, potentialMatchName, StringComparison.OrdinalIgnoreCase);
        }

        public Declaration FindEvent(Declaration module, string eventName)
        {
            var matches = MatchName(eventName);
            return matches.FirstOrDefault(m => module.Equals(Declaration.GetModuleParent(m)) && m.DeclarationType == DeclarationType.Event);
        }

        public Declaration FindLabel(Declaration procedure, string label)
        {
            var matches = MatchName(label);
            return matches.FirstOrDefault(m => procedure.Equals(m.ParentDeclaration) && m.DeclarationType == DeclarationType.LineLabel);
        }

        public IEnumerable<Declaration> MatchName(string name)
        {
            var normalizedName = name.ToLowerInvariant();
            Declaration[] result;
            if (_declarationsByName.TryGetValue(normalizedName, out result))
            {
                return result;
            }
            return Enumerable.Empty<Declaration>();
        }

        public Declaration FindProject(string name, Declaration currentScope = null)
        {
            Declaration result = null;
            try
            {
                result = MatchName(name).SingleOrDefault(project => project.DeclarationType.HasFlag(DeclarationType.Project)
                    && (currentScope == null || project.ProjectId == currentScope.ProjectId));
            }
            catch (InvalidOperationException exception)
            {
                Logger.Error(exception, "Multiple matches found for project '{0}'.", name);
            }

            return result;
        }

        public Declaration FindStdModule(string name, Declaration parent = null, bool includeBuiltIn = false)
        {
            Declaration result = null;
            try
            {
                var matches = MatchName(name);
                result = matches.SingleOrDefault(declaration => declaration.DeclarationType.HasFlag(DeclarationType.ProceduralModule)
                    && (parent == null || parent.Equals(declaration.ParentDeclaration))
                    && (includeBuiltIn || !declaration.IsBuiltIn));
            }
            catch (InvalidOperationException exception)
            {
                Logger.Error(exception, "Multiple matches found for std.module '{0}'.", name);
            }

            return result;
        }

        public Declaration FindReferencedProject(Declaration callingProject, string referencedProjectName)
        {
            return FindInReferencedProjectByPriority(callingProject, referencedProjectName, p => p.DeclarationType.HasFlag(DeclarationType.Project));
        }

        public Declaration FindModuleEnclosingProjectWithoutEnclosingModule(Declaration callingProject, Declaration callingModule, string calleeModuleName, DeclarationType moduleType)
        {
            var nameMatches = MatchName(calleeModuleName);
            var moduleMatches = nameMatches.Where(m =>
                m.DeclarationType.HasFlag(moduleType)
                && Declaration.GetProjectParent(m).Equals(callingProject)
                && !m.Equals(callingModule));
            var accessibleModules = moduleMatches.Where(calledModule => AccessibilityCheck.IsModuleAccessible(callingProject, callingModule, calledModule));
            var match = accessibleModules.FirstOrDefault();
            return match;
        }

        public Declaration FindDefaultInstanceVariableClassEnclosingProject(Declaration callingProject, Declaration callingModule, string defaultInstanceVariableClassName)
        {
            var nameMatches = MatchName(defaultInstanceVariableClassName);
            var moduleMatches = nameMatches.Where(m =>
                m.DeclarationType == DeclarationType.ClassModule && ((ClassModuleDeclaration)m).HasDefaultInstanceVariable
                && Declaration.GetProjectParent(m).Equals(callingProject));
            var accessibleModules = moduleMatches.Where(calledModule => AccessibilityCheck.IsModuleAccessible(callingProject, callingModule, calledModule));
            var match = accessibleModules.FirstOrDefault();
            return match;
        }

        public Declaration FindModuleReferencedProject(Declaration callingProject, Declaration callingModule, string calleeModuleName, DeclarationType moduleType)
        {
            var moduleMatches = FindAllInReferencedProjectByPriority(callingProject, calleeModuleName, p => p.DeclarationType.HasFlag(moduleType));
            var accessibleModules = moduleMatches.Where(calledModule => AccessibilityCheck.IsModuleAccessible(callingProject, callingModule, calledModule));
            var match = accessibleModules.FirstOrDefault();
            return match;
        }

        public Declaration FindModuleReferencedProject(Declaration callingProject, Declaration callingModule, Declaration referencedProject, string calleeModuleName, DeclarationType moduleType)
        {
            var moduleMatches = FindAllInReferencedProjectByPriority(callingProject, calleeModuleName, p => referencedProject.Equals(Declaration.GetProjectParent(p)) && p.DeclarationType.HasFlag(moduleType));
            var accessibleModules = moduleMatches.Where(calledModule => AccessibilityCheck.IsModuleAccessible(callingProject, callingModule, calledModule));
            var match = accessibleModules.FirstOrDefault();
            return match;
        }

        public Declaration FindDefaultInstanceVariableClassReferencedProject(Declaration callingProject, Declaration callingModule, string calleeModuleName)
        {
            var moduleMatches = FindAllInReferencedProjectByPriority(callingProject, calleeModuleName, p => p.DeclarationType == DeclarationType.ClassModule && ((ClassModuleDeclaration)p).HasDefaultInstanceVariable);
            var accessibleModules = moduleMatches.Where(calledModule => AccessibilityCheck.IsModuleAccessible(callingProject, callingModule, calledModule));
            var match = accessibleModules.FirstOrDefault();
            return match;
        }

        public Declaration FindDefaultInstanceVariableClassReferencedProject(Declaration callingProject, Declaration callingModule, Declaration referencedProject, string calleeModuleName)
        {
            var moduleMatches = FindAllInReferencedProjectByPriority(callingProject, calleeModuleName,
                p =>
                    referencedProject.Equals(Declaration.GetProjectParent(p))
                    && p.DeclarationType == DeclarationType.ClassModule && ((ClassModuleDeclaration)p).HasDefaultInstanceVariable);
            var accessibleModules = moduleMatches.Where(calledModule => AccessibilityCheck.IsModuleAccessible(callingProject, callingModule, calledModule));
            var match = accessibleModules.FirstOrDefault();
            return match;
        }

        public Declaration FindMemberWithParent(Declaration callingProject, Declaration callingModule, Declaration callingParent, Declaration parent, string memberName, DeclarationType memberType)
        {
            var allMatches = MatchName(memberName);
            var memberMatches = allMatches.Where(m =>
                m.DeclarationType.HasFlag(memberType)
                && parent.Equals(m.ParentDeclaration));
            var accessibleMembers = memberMatches.Where(m => AccessibilityCheck.IsMemberAccessible(callingProject, callingModule, callingParent, m));
            var match = accessibleMembers.FirstOrDefault();
            if (match != null)
            {
                return match;
            }
            foreach (var supertype in ClassModuleDeclaration.GetSupertypes(parent))
            {
                var supertypeMember = FindMemberWithParent(callingProject, callingModule, callingParent, supertype, memberName, memberType);
                if (supertypeMember != null)
                {
                    return supertypeMember;
                }
            }
            return null;
        }

        public Declaration FindMemberEnclosingModule(Declaration callingModule, Declaration callingParent, string memberName, DeclarationType memberType)
        {
            // We do not explicitly pass the callingProject here because we have to walk up the type hierarchy
            // and thus the project differs depending on the callingModule.
            var callingProject = Declaration.GetProjectParent(callingModule);
            var allMatches = MatchName(memberName);
            var memberMatches = allMatches.Where(m =>
                m.DeclarationType.HasFlag(memberType)
                && Declaration.GetProjectParent(m).Equals(callingProject)
                && callingModule.Equals(Declaration.GetModuleParent(m)));
            var accessibleMembers = memberMatches.Where(m => AccessibilityCheck.IsMemberAccessible(callingProject, callingModule, callingParent, m));
            var match = accessibleMembers.FirstOrDefault();
            if (match != null)
            {
                return match;
            }
            // Classes such as Worksheet have properties such as Range that can be access in a user defined class such as Sheet1,
            // that's why we have to walk the type hierarchy and find these implementations.
            foreach (var supertype in ClassModuleDeclaration.GetSupertypes(callingModule))
            {
                // Only built-in classes such as Worksheet can be considered "real base classes".
                // User created interfaces work differently and don't allow accessing accessing implementations.
                if (!supertype.IsBuiltIn)
                {
                    continue;
                }
                var supertypeMatch = FindMemberEnclosingModule(supertype, callingParent, memberName, memberType);
                if (supertypeMatch != null)
                {
                    return supertypeMatch;
                }
            }
            return match;
        }

        public Declaration FindMemberEnclosingProcedure(Declaration enclosingProcedure, string memberName, DeclarationType memberType, ParserRuleContext onSiteContext = null)
        {
            var allMatches = MatchName(memberName);
            var memberMatches = allMatches.Where(m =>
                m.DeclarationType.HasFlag(memberType)
                && enclosingProcedure.Equals(m.ParentDeclaration));
            return memberMatches.FirstOrDefault();
        }

        public Declaration OnUndeclaredVariable(Declaration enclosingProcedure, string identifierName, ParserRuleContext context)
        {
            var undeclaredLocal = new Declaration(new QualifiedMemberName(enclosingProcedure.QualifiedName.QualifiedModuleName, identifierName), enclosingProcedure, enclosingProcedure, "Variant", string.Empty, false, false, Accessibility.Implicit, DeclarationType.Variable, context, context.GetSelection(), false, null, false, null, null, true);

            var hasUndeclared = _undeclared.ContainsKey(enclosingProcedure.QualifiedName);
            if (hasUndeclared)
            {
                var inScopeUndeclared = _undeclared[enclosingProcedure.QualifiedName].FirstOrDefault(d => d.IdentifierName == identifierName);
                if (inScopeUndeclared != null)
                {
                    return inScopeUndeclared;
                }
                _undeclared[enclosingProcedure.QualifiedName].Add(undeclaredLocal);
            }
            else
            {
                _undeclared[enclosingProcedure.QualifiedName] = new List<Declaration> { undeclaredLocal };
            }

            return undeclaredLocal;
        }

        public Declaration FindMemberEnclosedProjectWithoutEnclosingModule(Declaration callingProject, Declaration callingModule, Declaration callingParent, string memberName, DeclarationType memberType)
        {
            var project = callingProject;
            var module = callingModule;
            var parent = callingParent;

            var allMatches = MatchName(memberName);
            var memberMatches = allMatches.Where(m =>
                m.DeclarationType.HasFlag(memberType)
                && Declaration.GetModuleParent(m).DeclarationType == DeclarationType.ProceduralModule
                && Declaration.GetProjectParent(m).Equals(callingProject)
                && !callingModule.Equals(Declaration.GetModuleParent(m)));
            var accessibleMembers = memberMatches.Where(m => AccessibilityCheck.IsMemberAccessible(callingProject, callingModule, callingParent, m));
            var match = accessibleMembers.FirstOrDefault();
            return match;
        }

        private static bool IsInstanceSensitive(DeclarationType memberType)
        {
            return memberType.HasFlag(DeclarationType.Variable) || memberType == DeclarationType.Constant || memberType.HasFlag(DeclarationType.Procedure) || memberType.HasFlag(DeclarationType.Function);
        }

        public Declaration FindMemberEnclosedProjectInModule(Declaration callingProject, Declaration callingModule, Declaration callingParent, Declaration memberModule, string memberName, DeclarationType memberType)
        {
            var allMatches = MatchName(memberName);
            var memberMatches = allMatches.Where(m =>
                m.DeclarationType.HasFlag(memberType)
                && Declaration.GetProjectParent(m).Equals(callingProject)
                && memberModule.Equals(Declaration.GetModuleParent(m)));
            var accessibleMembers = memberMatches.Where(m => AccessibilityCheck.IsMemberAccessible(callingProject, callingModule, callingParent, m));
            var match = accessibleMembers.FirstOrDefault();
            if (match != null)
            {
                return match;
            }
            foreach (var supertype in ClassModuleDeclaration.GetSupertypes(memberModule))
            {
                var supertypeMember = FindMemberEnclosedProjectInModule(callingProject, callingModule, callingParent, supertype, memberName, memberType);
                if (supertypeMember != null)
                {
                    return supertypeMember;
                }
            }
            return null;
        }

        public Declaration FindMemberReferencedProject(Declaration callingProject, Declaration callingModule, Declaration callingParent, string memberName, DeclarationType memberType)
        {
            bool isInstanceSensitive = IsInstanceSensitive(memberType);
            var memberMatches = FindAllInReferencedProjectByPriority(callingProject, memberName, p => (!isInstanceSensitive || Declaration.GetModuleParent(p) == null || Declaration.GetModuleParent(p).DeclarationType != DeclarationType.ClassModule) && p.DeclarationType.HasFlag(memberType));
            var accessibleMembers = memberMatches.Where(m => AccessibilityCheck.IsMemberAccessible(callingProject, callingModule, callingParent, m));
            var match = accessibleMembers.FirstOrDefault();
            return match;
        }

        public Declaration FindMemberReferencedProjectInModule(Declaration callingProject, Declaration callingModule, Declaration callingParent, DeclarationType moduleType, string memberName, DeclarationType memberType)
        {
            var memberMatches = FindAllInReferencedProjectByPriority(callingProject, memberName, p => p.DeclarationType.HasFlag(memberType) && (Declaration.GetModuleParent(p) == null || Declaration.GetModuleParent(p).DeclarationType == moduleType));
            var accessibleMembers = memberMatches.Where(m => AccessibilityCheck.IsMemberAccessible(callingProject, callingModule, callingParent, m));
            var match = accessibleMembers.FirstOrDefault();
            return match;
        }

        public Declaration FindMemberReferencedProjectInGlobalClassModule(Declaration callingProject, Declaration callingModule, Declaration callingParent, string memberName, DeclarationType memberType)
        {
            var memberMatches = FindAllInReferencedProjectByPriority(callingProject, memberName, p => p.DeclarationType.HasFlag(memberType) && (Declaration.GetModuleParent(p) == null || Declaration.GetModuleParent(p).DeclarationType == DeclarationType.ClassModule) && ((ClassModuleDeclaration)Declaration.GetModuleParent(p)).IsGlobalClassModule);
            var accessibleMembers = memberMatches.Where(m => AccessibilityCheck.IsMemberAccessible(callingProject, callingModule, callingParent, m));
            var match = accessibleMembers.FirstOrDefault();
            return match;
        }

        public Declaration FindMemberReferencedProjectInModule(Declaration callingProject, Declaration callingModule, Declaration callingParent, Declaration memberModule, string memberName, DeclarationType memberType)
        {
            var memberMatches = FindAllInReferencedProjectByPriority(callingProject, memberName, p => p.DeclarationType.HasFlag(memberType) && memberModule.Equals(Declaration.GetModuleParent(p)));
            var accessibleMembers = memberMatches.Where(m => AccessibilityCheck.IsMemberAccessible(callingProject, callingModule, callingParent, m));
            var match = accessibleMembers.FirstOrDefault();
            if (match != null)
            {
                return match;
            }
            foreach (var supertype in ClassModuleDeclaration.GetSupertypes(memberModule))
            {
                var supertypeMember = FindMemberReferencedProjectInModule(callingProject, callingModule, callingParent, supertype, memberName, memberType);
                if (supertypeMember != null)
                {
                    return supertypeMember;
                }
            }
            return null;
        }

        public Declaration FindMemberReferencedProject(Declaration callingProject, Declaration callingModule, Declaration callingParent, Declaration referencedProject, string memberName, DeclarationType memberType)
        {
            var memberMatches = FindAllInReferencedProjectByPriority(callingProject, memberName, p => p.DeclarationType.HasFlag(memberType) && referencedProject.Equals(Declaration.GetProjectParent(p)));
            var accessibleMembers = memberMatches.Where(m => AccessibilityCheck.IsMemberAccessible(callingProject, callingModule, callingParent, m));
            var match = accessibleMembers.FirstOrDefault();
            return match;
        }

        private Declaration FindInReferencedProjectByPriority(Declaration enclosingProject, string name, Func<Declaration, bool> predicate)
        {
            return FindAllInReferencedProjectByPriority(enclosingProject, name, predicate).FirstOrDefault();
        }

        private IEnumerable<Declaration> FindAllInReferencedProjectByPriority(Declaration enclosingProject, string name, Func<Declaration, bool> predicate)
        {
            var interprojectMatches = MatchName(name).Where(predicate).ToList();
            var projectReferences = ((ProjectDeclaration)enclosingProject).ProjectReferences.ToList();
            if (interprojectMatches.Count == 0)
            {
                yield break;
            }
            foreach (var projectReference in projectReferences)
            {
                var match = interprojectMatches.FirstOrDefault(interprojectMatch => interprojectMatch.ProjectId == projectReference.ReferencedProjectId);
                if (match != null)
                {
                    yield return match;
                }
            }
        }
    }
}
