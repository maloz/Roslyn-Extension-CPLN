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
    [ExportCodeFixProvider(DiagnosticAnalyzerComment.DiagnosticId, LanguageNames.CSharp)]
    internal class CodeFixeComment : ICodeFixProvider
    {
        public IEnumerable<string> GetFixableDiagnosticIds()
        {
            return new[] { DiagnosticAnalyzerComment.DiagnosticId };
        }

        public async Task<IEnumerable<CodeAction>> GetFixesAsync(Document document, TextSpan span, IEnumerable<Diagnostic> diagnostics, CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken);   /*Document à utiliser (root) et surtout à modifier*/

            var trivia = root.FindTrivia(span.Start);/*Variable qui contiendra tour à tour les différents trivia code*/


            if (trivia.IsKind(SyntaxKind.SingleLineCommentTrivia)) /*Si c'est un commentaire sur une ligne */
            {
                var singleTrivia = (SyntaxTrivia)trivia;

                var commentContent = singleTrivia.ToString().Replace("//", string.Empty); /*Récupération du contenu du commentaire*/
                var newComment = SyntaxFactory.Comment(string.Format("/* {0} */", commentContent));/*Ajout des nouvelles balises */
                var newRoot = root.ReplaceTrivia(singleTrivia, newComment); /* Création du nouvel arbre*/
                return new[] { CodeAction.Create("Convertir dans le bon format", document.WithSyntaxRoot(newRoot)) };/*Renvoi du nouvel arbre */
            }
            

            return null;
        }




    }

}