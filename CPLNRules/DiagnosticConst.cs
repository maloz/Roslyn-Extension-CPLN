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
    public class DiagnosticAnalyzerConst : ISyntaxNodeAnalyzer<SyntaxKind>
    {
        internal const string DiagnosticId = "ConstDiagnostic";
        internal const string Description = "Mauvais formattage de la constante";
        internal const string MessageFormat = "{0}";
        internal const string Category = "Correction";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Description, MessageFormat, Category, DiagnosticSeverity.Warning); /* Variable contenant le message transmis à l'utilisateur */

        public ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get { return ImmutableArray.Create(Rule); }
        }
        public ImmutableArray<SyntaxKind> SyntaxKindsOfInterest /*Ce qui nous intéresse à analyser*/
        {
            get
            {
                return ImmutableArray.Create(SyntaxKind.LocalDeclarationStatement, SyntaxKind.ConstKeyword); /*Les déclarations de variables ainsi que le mot clef Const */
            }
        }
        public void AnalyzeNode(SyntaxNode node, SemanticModel model, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken) /* Méthode d'analyse des nodes*/
        {

            var localDeclarationConst = node as LocalDeclarationStatementSyntax; /* Requete Linq, récupération des déclarations */
            if (localDeclarationConst != null &&
                localDeclarationConst.Modifiers.Any(SyntaxKind.ConstKeyword) /*Vérification que c'est bien une constante */
                )
            {
                foreach (VariableDeclaratorSyntax variable in localDeclarationConst.Declaration.Variables)
                {
                    string varName = variable.Identifier.Text;
                    if (!varName.Equals(varName.ToUpper())) /* Si le nom n'est pas déjà tout en majuscule */
                    {
                        addDiagnostic(Diagnostic.Create(Rule, variable.GetLocation(), "Les constantes s'écrivent totalement en majuscule")); /*Ajout d'un diagnostic */
                    }

                }

            }

        }
    }


}
