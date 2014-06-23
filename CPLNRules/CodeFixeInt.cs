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
using Microsoft.CodeAnalysis.Options;

namespace CPLNRules
{
    [ExportCodeFixProvider(DiagnosticAnalyzerInt.DiagnosticId, LanguageNames.CSharp)] /*Reflector*/
    internal class CodeFixeInt : ICodeFixProvider
    {
        public IEnumerable<string> GetFixableDiagnosticIds()
        {
            return new[] { DiagnosticAnalyzerInt.DiagnosticId };
        }

        public async Task<IEnumerable<CodeAction>> GetFixesAsync(Document document, TextSpan span, IEnumerable<Diagnostic> diagnostics, CancellationToken cancellationToken)
        {

            
            /* Toute les explications sur le fichier CodeFixeString.cs  
               Les instructions sontr très similaires. */
            var root = await document.GetSyntaxRootAsync(cancellationToken); 
            var token = root.FindToken(span.Start); 
            var node = root.FindNode(span);



            if (node.IsKind(SyntaxKind.VariableDeclarator))
            {
                if (node.Parent.Parent is LocalDeclarationStatementSyntax)
                {
                    var Verify = (LocalDeclarationStatementSyntax)node.Parent.Parent;
                    var Verify2 = (VariableDeclarationSyntax)node.Parent;
                    if (!Verify.Modifiers.Any(SyntaxKind.ConstKeyword))
                    {
                        if (Verify2.Type.ToString() == "int")
                        {

                            var variable = (VariableDeclaratorSyntax)node;
                            string newName = variable.Identifier.ValueText;
                            if (newName.Length > 1)
                            {
                                newName = char.ToUpper(newName[0]) + newName.Substring(1);
                            }

                            newName = "i" + newName;

                            var semanticModel = await document.GetSemanticModelAsync(cancellationToken);
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

                            var newSolution = await Renamer.RenameSymbolAsync(solution, symbol, newName, options);

                            return new[] { CodeAction.Create("Ajouter i", newSolution )};
                            
                        }
                    }
                }
            }

            return null;
        }




    }

}