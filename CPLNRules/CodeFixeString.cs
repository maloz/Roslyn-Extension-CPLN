using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.Formatting;
using System;

namespace CPLNRules
{
    [ExportCodeFixProvider(DiagnosticAnalyzerString.DiagnosticId, LanguageNames.CSharp)]
    internal class CodeFixeString : ICodeFixProvider
    {
        public IEnumerable<string> GetFixableDiagnosticIds()
        {
            return new[] { DiagnosticAnalyzerString.DiagnosticId };
        }

        public async Task<IEnumerable<CodeAction>> GetFixesAsync(Document document, TextSpan span, IEnumerable<Diagnostic> diagnostics, CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken);  /*Document à utiliser (root) et surtout à modifier*/
            var token = root.FindToken(span.Start);  /*Variable qui contiendra tour à tour les différents token des nodes du code*/
            var node = root.FindNode(span); /*Variable qui contiendra tour à tour les différents nodes du code*/

            #region string
            if (node.IsKind(SyntaxKind.VariableDeclarator)) /* Si c'est une "déclaration" de variable (i = 1)*/
            {
                if (node.Parent.Parent is LocalDeclarationStatementSyntax) /* Si le parent de son parent est une déclaration de variable (int i = 1;)*/
                {
                    var Verify = (LocalDeclarationStatementSyntax)node.Parent.Parent; /* Récupération du parent du parent*/
                    var Verify2 = (VariableDeclarationSyntax)node.Parent; /* Récupération du parent */
                    if (!Verify.Modifiers.Any(SyntaxKind.ConstKeyword)) /* Analyse du parent de parent pour le mot clef const */
                    {
                        if (Verify2.Type.ToString() == "string") /* Analyse du parent pour le type*/
                        {   
                          var variable = (VariableDeclaratorSyntax)node; /*Récupération du node */
                          string newName = variable.Identifier.ValueText; /*Récupération du nom */

                          if (newName.Length > 1)
                          {
                              newName = char.ToUpper(newName[0]) + newName.Substring(1); /* Premier caractère en Maj*/
                          }

                          newName = "str" + newName; /* Ajout du préfixe 'str'*/

                          var semanticModel = await document.GetSemanticModelAsync(cancellationToken); /*Récupération des 4 variables obligatoires pour créer une nouvelle solution*/
                          if (semanticModel == null)                                                   /* Tests si null facultatifs mais par sécurité ils sont effectués*/
                              return null;

                          var symbol = semanticModel.GetDeclaredSymbol(variable, cancellationToken);
                          if (symbol == null)
                              return null;

                          var solution = document.Project.Solution;
                          if (solution == null)
                          {
                              return null;
                          }
                          var options = solution.Workspace.GetOptions();
                          var newSolution = await Renamer.RenameSymbolAsync(solution, symbol, newName, options); /*Création de la nouvelle solution en appelent RenameSymbolAsync qui renomera la variable partout*/

                          return new[] { CodeAction.Create("Ajouter str", newSolution) }; /*Envoi de la nouvelle solution */
                        }
                    }
                }
            }
            #endregion


            return null;
        }




    }

}