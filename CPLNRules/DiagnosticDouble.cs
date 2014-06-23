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
    public class DiagnosticAnalyzerDouble : ISyntaxNodeAnalyzer<SyntaxKind>
    {
        internal const string DiagnosticId = "DoubleDiagnostic";
        internal const string Description = "Mauvais formattage du double";
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
                return ImmutableArray.Create(SyntaxKind.LocalDeclarationStatement); /*Les déclarations de variables */
            }
        }
        public void AnalyzeNode(SyntaxNode node, SemanticModel model, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken) /* Méthode d'analyse des nodes*/
        {

            var localDeclarationDouble = node as LocalDeclarationStatementSyntax; /* Requete Linq, récupération des déclarations */
            if (localDeclarationDouble != null &&
                localDeclarationDouble.Declaration.Type.ToString() == "double" && /*Si c'est bien un double */
                !localDeclarationDouble.Modifiers.Any(SyntaxKind.ConstKeyword)) /* Si c'est pas une constante */
            {
                foreach (VariableDeclaratorSyntax variable in localDeclarationDouble.Declaration.Variables)
                {
                    string varName = variable.Identifier.Text; /* Récupération du nom de la variable */
                    if (varName.Length > 2)
                    {
                        if (varName[0] == 'd' && varName[1] == 'b' && varName[2] == 'l') /*Si elle ne commence pas déjà par 'dbl' */
                        {
                        }
                        else
                        {
                            addDiagnostic(Diagnostic.Create(Rule, variable.GetLocation(), "Les variables de nombre réels doivent être préfixé de 'dbl'"));  /* Ajout d'un diagnostic */
                        }
                    }
                    else /* Si elle fait moins de 3 caractères ('dbl')*/
                    {
                        addDiagnostic(Diagnostic.Create(Rule, variable.GetLocation(), "Les variables de nombre réels doivent être préfixé de 'dbl'"));  /* Ajout d'un diagnostic */
                    }
                }
            }


        }
    }
    

}
