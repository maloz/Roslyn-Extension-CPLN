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
    public class DiagnosticAnalyzerInt : ISyntaxNodeAnalyzer<SyntaxKind> /* Héritage de ISyntaxNodeAnalyzer car analyse de node et pas de trivia*/
    {
        internal const string DiagnosticId = "IntDiagnostic";
        internal const string Description = "Mauvais formattage du int";
        internal const string MessageFormat = "{0}";
        internal const string Category = "Correction";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Description, MessageFormat, Category, DiagnosticSeverity.Warning);/* Variable contenant le message transmis à l'utilisateur */

        public ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get { return ImmutableArray.Create(Rule); }
        }
        public ImmutableArray<SyntaxKind> SyntaxKindsOfInterest /*Ce qui nous intéresse à analyser*/
        {
            get
            {
                return ImmutableArray.Create(SyntaxKind.LocalDeclarationStatement); /*Les déclarations de variables */
            }
        }
        public void AnalyzeNode(SyntaxNode node, SemanticModel model, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken) /* Méthode d'analyse des nodes*/
        {

            var localDeclarationInt = node as LocalDeclarationStatementSyntax; /* Requete Linq, récupération des déclarations */
            if (localDeclarationInt != null &&
                localDeclarationInt.Declaration.Type.ToString() == "int" && /*Si c'est bien un int */
                !localDeclarationInt.Modifiers.Any(SyntaxKind.ConstKeyword))  /*Si c'est pas une constante */
            {
                foreach (VariableDeclaratorSyntax variable in localDeclarationInt.Declaration.Variables)
                {
                    string varName = variable.Identifier.Text;
                    if (varName.Length > 1)
                    {
                        if (varName[0] != 'i') /*Si elle ne commence pas déjà par 'i' */
                        {
                            addDiagnostic(Diagnostic.Create(Rule, variable.GetLocation(), "Les variables de nombre entier doivent être préfixés d'un 'i'")); /* Ajout d'un diagnostic */
                        }
                    }
                }
            }



        }
    }


}
