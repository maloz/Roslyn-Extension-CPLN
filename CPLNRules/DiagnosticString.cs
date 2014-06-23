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
    public class DiagnosticAnalyzerString : ISyntaxNodeAnalyzer<SyntaxKind> /* Héritage de ISyntaxNodeAnalyzer car analyse de node et pas de trivia*/
    {
        internal const string DiagnosticId = "StringDiagnostic";
        internal const string Description = "Mauvais formattage du string";
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
                return ImmutableArray.Create(SyntaxKind.LocalDeclarationStatement); /*Declaration de variable*/
            }
        }
        public void AnalyzeNode(SyntaxNode node, SemanticModel model, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken) /*Analyse des Nodes */
        {

            var localDeclarationString = node as LocalDeclarationStatementSyntax; /* Requete Linq, récupération des déclarations */
            if (localDeclarationString != null &&
                localDeclarationString.Declaration.Type.ToString() == "string" && /*Si c'est bien un string */
                !localDeclarationString.Modifiers.Any(SyntaxKind.ConstKeyword)) /* Si c'est pas une constante */
            {
                foreach (VariableDeclaratorSyntax variable in localDeclarationString.Declaration.Variables)
                {
                    string varName = variable.Identifier.Text; /*Récupération du nom de la variable */
                    if (varName.Length > 2)
                    {
                        if (varName[0] == 's' && varName[1] == 't'  && varName[2] == 'r') /*Si elle ne commence pas déjà par str */
                        {
                        }
                        else
                        {
                            addDiagnostic(Diagnostic.Create(Rule, variable.GetLocation(), "Les variables de type Chaînes de caractères être préfixées de 'str'")); /*Ajout d'un diagnostic */
                        }
                    }
                    else /* Ajout d'un diagnostic si le nom fait moins de 3 caractères ('str')*/
                    {
                        addDiagnostic(Diagnostic.Create(Rule, variable.GetLocation(), "Les variables de type Chaînes de caractères être préfixées de 'str'")); /*Ajout d'un diagnostic */
                    }
                }
            }

        }
    }

}
