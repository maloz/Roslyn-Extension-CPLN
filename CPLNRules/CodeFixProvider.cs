using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.Formatting;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Rename;

namespace CPLNRules
{
    [ExportCodeFixProvider(DiagnosticAnalyzer.DiagnosticId, LanguageNames.CSharp)]
    internal class CodeFixProvider : ICodeFixProvider /*structure CodeFix*/
    {
        public IEnumerable<string> GetFixableDiagnosticIds()
        {
            return new[] { DiagnosticAnalyzer.DiagnosticId };
        }

        public async Task<IEnumerable<CodeAction>> GetFixesAsync(Document document, TextSpan span, IEnumerable<Diagnostic> diagnostics, CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken); /*Document à utiliser (root) dont une copie modifier sera envoyer en retour*/
            var token = root.FindToken(span.Start); /*Variable qui contiendra tour à tour les différents token des nodes du code*/
            var node = root.FindNode(span); /*Variable qui contiendra tour à tour les différents nodes du code*/

            #region If & Else
            if (token.IsKind(SyntaxKind.IfKeyword)) /*pour chaque token if */
            {
                var ifStatement = (IfStatementSyntax)token.Parent;
                var newIfStatement = ifStatement /*Création d'un nouveau ifstatement */
                    .WithStatement(SyntaxFactory.Block(ifStatement.Statement)) /*Ajout d'un block */
                    .WithAdditionalAnnotations(Formatter.Annotation); /*Permet une indentation correct */

                var newRoot = root.ReplaceNode(ifStatement, newIfStatement); /*Remplacement du root par un nouveau (pas modification mais remplacement! */

                return new[] { CodeAction.Create("Ajouter des crochets", document.WithSyntaxRoot(newRoot)) }; /* renvoi du nouvel arbre syntaxique */
            }

            if (token.IsKind(SyntaxKind.ElseKeyword))   /*pour chaque token else */
            {
                var elseClause = (ElseClauseSyntax)token.Parent;
                var newElseClause = elseClause
                    .WithStatement(SyntaxFactory.Block(elseClause.Statement))
                    .WithAdditionalAnnotations(Formatter.Annotation);

                var newRoot = root.ReplaceNode(elseClause, newElseClause);

                return new[] { CodeAction.Create("Ajouter des crochets", document.WithSyntaxRoot(newRoot)) };
            }
            #endregion


            #region Incrementation

            if (node.IsKind(SyntaxKind.SimpleAssignmentExpression)) /* Pour chaque calcul simple, dont les "mauvaises" incrémentations(trouvé grâce à l'arbre syntaxique */
            {
                var IncrementationClause = (BinaryExpressionSyntax)node; /* Récupération du node de la mauvais incrémentation */
                var IncrementionClauseExpressionStatement = IncrementationClause.Parent; /*Récupération du parent qui est un ExpressionStatement*/

                string right = IncrementationClause.Right.ToString(); /* récupération de ce qui est à droite du signe = */
                string left = IncrementationClause.Left.ToString();/* récupération de ce qui est à gauche du signe = */

                string RW = right.Replace(" ", string.Empty);/*Pour ensuite analyser si c'est une incrémentation ou décrémentation sans risque d'erreur à cause d'espace n'importe ou */
                string LW = left.Replace(" ", string.Empty);

                var ExpressionNew = SyntaxFactory.ExpressionStatement(IncrementationClause); /*Création de la variable ExpressionNew, qui ne peux pas être créé dans les if */

                var leading = IncrementionClauseExpressionStatement.GetLeadingTrivia();
                var trailing = IncrementionClauseExpressionStatement.GetTrailingTrivia();

                if (RW == LW + "-1") /*Si c'est une décrémentation*/
                {
                    var BonneIncrementation = SyntaxFactory.PostfixUnaryExpression(SyntaxKind.PostDecrementExpression, IncrementationClause.Left); /* Decrementation correcte */
                    ExpressionNew = SyntaxFactory.ExpressionStatement(BonneIncrementation).WithAdditionalAnnotations(Formatter.Annotation).WithLeadingTrivia(leading).WithTrailingTrivia(trailing); /* Création du parent = ExpressionStatement */
                }
                else /*Si c'est une incrémentation*/
                {
                    var BonneIncrementation = SyntaxFactory.PostfixUnaryExpression(SyntaxKind.PostIncrementExpression, IncrementationClause.Left); /* Incrementation correcte*/
                    ExpressionNew = SyntaxFactory.ExpressionStatement(BonneIncrementation).WithAdditionalAnnotations(Formatter.Annotation).WithLeadingTrivia(leading).WithTrailingTrivia(trailing); /* Création du parent = ExpressionStatement */
                }
                            
                var newRoot = root.ReplaceNode(IncrementionClauseExpressionStatement, ExpressionNew); /* Remplacement d'un ExpressionStatement par un autre */

                return new[] { CodeAction.Create("Changer la mise en forme", document.WithSyntaxRoot(newRoot)) };
            }

            #endregion


            #region MajusculeMethode
            if(node.IsKind(SyntaxKind.MethodDeclaration)) /* Pour chaque déclaration de méthode */
            {
                    var variable = (MethodDeclarationSyntax)node; /*Récupération du node */
                    string newName = variable.Identifier.ValueText; /* Récupération du nom de la méthode */
                    if (newName.Length > 1)
                    {
                        newName = char.ToUpper(newName[0]) + newName.Substring(1);
                    }
                    var semanticModel = await document.GetSemanticModelAsync(cancellationToken); /* 4 variables indispensables pour une nouvelle solution*/
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
                   
                    return new[] { CodeAction.Create("Mettre la première lettre en majuscule", newSolution) }; /* Code action pour remplacer la solution par une nouvelle afin de changer le nom de la méthode partout */

            }

            #endregion

            return null;
        }

    }
    
}