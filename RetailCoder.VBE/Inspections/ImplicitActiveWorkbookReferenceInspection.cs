using System.Collections.Generic;
using System.Linq;
using Rubberduck.Parsing.Symbols;
using Rubberduck.Parsing.VBA;
using Rubberduck.VBEditor.Application;
using Rubberduck.VBEditor.Extensions;
using Rubberduck.VBEditor.SafeComWrappers.Abstract;

namespace Rubberduck.Inspections
{
    public sealed class ImplicitActiveWorkbookReferenceInspection : InspectionBase
    {
        private readonly IHostApplication _hostApp;

        public ImplicitActiveWorkbookReferenceInspection(IVBE vbe, RubberduckParserState state)
            : base(state)
        {
            _hostApp = vbe.HostApplication();
        }

        public override string Meta { get { return InspectionsUI.ImplicitActiveWorkbookReferenceInspectionMeta; } }
        public override string Description { get { return InspectionsUI.ImplicitActiveWorkbookReferenceInspectionResultFormat; } }
        public override CodeInspectionType InspectionType { get { return CodeInspectionType.MaintainabilityAndReadabilityIssues; } }

        private static readonly string[] Targets =
        {
            "Worksheets", "Sheets", "Names", "_Default"
        };

        private static readonly string[] ParentScopes =
        {
            "EXCEL.EXE;Excel._Global",
            "EXCEL.EXE;Excel._Application",
            "EXCEL.EXE;Excel.Sheets",
            //"EXCEL.EXE;Excel.Worksheets",
        };

        public override IEnumerable<InspectionResultBase> GetInspectionResults()
        {
            if (_hostApp == null || _hostApp.ApplicationName != "Excel")
            {
                return Enumerable.Empty<InspectionResultBase>();
                // if host isn't Excel, the ExcelObjectModel declarations shouldn't be loaded anyway.
            }

            var issues = BuiltInDeclarations
                .Where(item => ParentScopes.Contains(item.ParentScope) 
                    && item.References.Any(r => Targets.Contains(r.IdentifierName)))
                .SelectMany(declaration => declaration.References.Distinct())
                .Where(item => Targets.Contains(item.IdentifierName))
                .ToList();

            return issues.Select(issue =>
                new ImplicitActiveWorkbookReferenceInspectionResult(this, issue));
        }
    }

    public class ImplicitActiveWorkbookReferenceInspectionResult : InspectionResultBase
    {
        private readonly IdentifierReference _reference;
        private readonly IEnumerable<CodeInspectionQuickFix> _quickFixes;

        public ImplicitActiveWorkbookReferenceInspectionResult(IInspection inspection, IdentifierReference reference)
            : base(inspection, reference.QualifiedModuleName, reference.Context)
        {
            _reference = reference;
            _quickFixes = new CodeInspectionQuickFix[]
            {
                new IgnoreOnceQuickFix(reference.Context, QualifiedSelection, Inspection.AnnotationName),
            };
        }

        public override IEnumerable<CodeInspectionQuickFix> QuickFixes { get { return _quickFixes; } }

        public override string Description
        {
            get { return string.Format(Inspection.Description, Context.GetText() /*_reference.Declaration.IdentifierName*/); }
        }
    }
}
