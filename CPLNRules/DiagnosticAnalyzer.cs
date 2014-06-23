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
    public class DiagnosticAnalyzer : ISyntaxNodeAnalyzer<SyntaxKind>
    {
        internal const string DiagnosticId = "CodeFix";
        internal const string Description = "Mauvais formattage";
        internal const string MessageFormat = "{0}";
        internal const string Category = "Correction";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Description, MessageFormat, Category, DiagnosticSeverity.Warning);/* Variable contenant le message transmis � l'utilisateur */

        public ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get { return ImmutableArray.Create(Rule); }
        }
        public ImmutableArray<SyntaxKind> SyntaxKindsOfInterest /*Ce qui nous intéresse à analyser */
        {
            get
            {
                return ImmutableArray.Create(SyntaxKind.IfStatement, SyntaxKind.ElseClause, SyntaxKind.SimpleAssignmentExpression, SyntaxKind.MethodDeclaration); /*Les mots clefs if et else, les calculs et les d�clarations de m�thode */
            }
        }
        public void AnalyzeNode(SyntaxNode node, SemanticModel model, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken) /*Analyse des nodes */
        {
            #region If&Else
            var ifStatement = node as IfStatementSyntax; /*Récupération des IfStatement parmis tous les nodes */
            if (ifStatement != null &&
                ifStatement.Statement != null &&
                !ifStatement.Statement.IsKind(SyntaxKind.Block)) /*Si il ne contient pas un block comme il devrait (block = crochets) */
            {
                addDiagnostic(Diagnostic.Create(Rule, ifStatement.IfKeyword.GetLocation(), "Mettre sur deux lignes une instruction if suivi d'une instruction ; la seconde ligne étant indentée")); /*Ajout de diagnostic */
            }

            var elseClause = node as ElseClauseSyntax; /*Récupération des Else parmis tous les nodes*/
            if (elseClause != null &&
                elseClause.Statement != null &&
                !elseClause.Statement.IsKind(SyntaxKind.Block) && /*Pas que ce soit d�j� un block avec {}*/
                !elseClause.Statement.IsKind(SyntaxKind.IfStatement)) /* Pas que le else sois dans un else if */
            {
                addDiagnostic(Diagnostic.Create(Rule, elseClause.ElseKeyword.GetLocation(), "Mettre sur deux lignes une instruction else suivi d'une instruction ; la seconde ligne étant indentée")); /*Ajout de diagnostic */
            }
            #endregion

            #region MajMethode
            var methode = node as MethodDeclarationSyntax; /* Linq: Récupération des déclaration de méthode */
            if (methode != null)
            {
                string varName = methode.Identifier.Text; /* Récupération du nom */
                if (!char.IsUpper(varName[0])) /* Si le premier caractère est pas en majuscule*/
                {
                    addDiagnostic(Diagnostic.Create(Rule, methode.Identifier.GetLocation(), "Les méthode doivent commencer par une majusucule")); /* Ajout d'un diagnostic */
                }                
            }
            #endregion

            var incrementation = node as BinaryExpressionSyntax;/* Récupération des calculs simples*/
            if (incrementation != null)
            {
                
               string right = incrementation.Right.ToString(); /* Récupération de ce qui est à droite du signe = */
               string left = incrementation.Left.ToString();/* Récupération de ce qui est à gauche du signe = */

               string RW = right.Replace(" ", string.Empty); /* Si l'utilisateur met des espaces, la condition pourra quand même être remplie correctement */
               string LW = left.Replace(" ", string.Empty);
               

                if (RW == LW + "-1" || /*Si c'est une décrementation mal faite ou incrémentation*/
                  RW == LW + "+1"
                 )
               {
                addDiagnostic(Diagnostic.Create(Rule, incrementation.GetLocation(), "Utiliser les facilités du langage pour une incrémentation plus courte")); /* Ajout d'un diagnostic */
               }
               
            }
        }
    }
    

}
