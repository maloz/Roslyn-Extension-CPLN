using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CPLNRules
{
    [DiagnosticAnalyzer]
    [ExportDiagnosticAnalyzer(DiagnosticId, LanguageNames.CSharp)]
    public class DiagnosticAnalyzerComment : ISyntaxTreeAnalyzer
    {
        internal const string DiagnosticId = "DiagnosticComment";
        internal const string Description = "Les commentaires doivent être placés entre /* et *//";
        internal const string MessageFormat = "{0}";
        internal const string Category = "Renommage";

        internal static DiagnosticDescriptor RuleComment = new DiagnosticDescriptor(DiagnosticId, Description, MessageFormat, Category, DiagnosticSeverity.Warning); /* Variable contenant le message transmis à l'utilisateur */

        public ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get { return ImmutableArray.Create(RuleComment); }
        }

        public void AnalyzeSyntaxTree(SyntaxTree tree, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken) /*Analyse des Trivias*/
        {
            var root = tree.GetRoot(); /*Récupération de l'arbre syntaxique */
            var trivia = root.DescendantTrivia(); /*récupération des trivias */
            var a = trivia.Where(x => x.IsKind(SyntaxKind.SingleLineCommentTrivia)).ToList(); /*Requête Lamba. Pour chaque commentaire en single line (//) */

            foreach (var b in a)
            {
                addDiagnostic(Diagnostic.Create(RuleComment, b.GetLocation(), "Les commentaires doivent être placés entre /* et */")); /*Ajout d'un diagnostic */
            }
        }
    }
}
