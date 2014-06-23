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
    [ExportCodeFixProvider(DiagnosticAnalyzerConst.DiagnosticId, LanguageNames.CSharp)]
    internal class CodeFixeConst : ICodeFixProvider
    {
        public IEnumerable<string> GetFixableDiagnosticIds()
        {
            return new[] { DiagnosticAnalyzerConst.DiagnosticId };
        }

        public async Task<IEnumerable<CodeAction>> GetFixesAsync(Document document, TextSpan span, IEnumerable<Diagnostic> diagnostics, CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken);  /*Document à utiliser (root) et surtout à modifier*/
            var token = root.FindToken(span.Start);  /*Variable qui contiendra tour à tour les différents token des nodes du code*/
            var node = root.FindNode(span);  /*Variable qui contiendra tour à tour les différents nodes du code*/

            if (node.IsKind(SyntaxKind.VariableDeclarator)) /* Si c'est une "déclaration" de variable (i = 1)*/
            {
               var variable = (VariableDeclaratorSyntax)node; /*Récupération du node */
               string strNewName = variable.Identifier.ValueText; /*Récupération du nom */
               string strNameDone = String.Empty;
               for (int i = 0; i < strNewName.Length; i++)
               {
                   strNameDone = strNameDone + char.ToUpper(strNewName[i]); /*Mise en majuscule du nom dans un nouveau string */
               }

                var semanticModel = await document.GetSemanticModelAsync(cancellationToken); /* Déclaration des 4 variables indispensable pour créer la nouvelle solution */
                if (semanticModel == null)
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

                var newSolution = await Renamer.RenameSymbolAsync(solution, symbol, strNameDone, options); /* Changement du nom de la variable dans toute la solution */

                return new[] { CodeAction.Create("Mettre en majuscule", newSolution) }; /* Renvoi de la nouvelle solution*/


            }
            return null;
        }




    }

}