using Bridge.Contract;
using ICSharpCode.NRefactory.CSharp;

namespace Bridge.Translator.Lua
{
    public class BreakBlock : AbstractEmitterBlock
    {
        public BreakBlock(IEmitter emitter, BreakStatement breakStatement)
            : base(emitter, breakStatement)
        {
            this.Emitter = emitter;
            this.BreakStatement = breakStatement;
        }

        public BreakBlock(IEmitter emitter, YieldBreakStatement breakStatement)
            : base(emitter, breakStatement)
        {
            this.Emitter = emitter;
            this.YieldBreakStatement = breakStatement;
        }

        public BreakStatement BreakStatement
        {
            get;
            set;
        }

        public YieldBreakStatement YieldBreakStatement
        {
            get;
            set;
        }

        protected override void DoEmit()
        {
            if (this.Emitter.JumpStatements != null)
            {
                var finallyNode = this.GetParentFinallyBlock(this.BreakStatement ?? (AstNode)this.YieldBreakStatement, true);

                if (finallyNode != null)
                {
                    var hashcode = finallyNode.GetHashCode();
                    this.Emitter.AsyncBlock.JumpLabels.Add(new AsyncJumpLabel
                    {
                        Node = finallyNode,
                        Output = this.Emitter.Output
                    });
                    this.Write("$step = ${" + hashcode + "};");
                    this.WriteNewLine();
                    this.Write("$jumpFromFinally = ");
                    this.Emitter.JumpStatements.Add(new JumpInfo(this.Emitter.Output, this.Emitter.Output.Length, true));
                    this.WriteSemiColon();
                    this.WriteNewLine();
                }
                else
                {
                    this.Write("$step = ");
                    this.Emitter.JumpStatements.Add(new JumpInfo(this.Emitter.Output, this.Emitter.Output.Length, true));

                    this.WriteSemiColon();
                    this.WriteNewLine();
                }

                this.Write("continue");
            }
            else
            {
                if (this.Emitter.ReplaceJump)
                {
                    var found = false;
                    this.BreakStatement.GetParent(n =>
                    {
                        if (n is SwitchStatement)
                        {
                            found = true;
                            return true;
                        }

                        if (n is ForStatement || n is ForeachStatement || n is WhileStatement || n is DoWhileStatement || n is AnonymousMethodExpression || n is LambdaExpression)
                        {
                            found = false;
                            return true;
                        }

                        return false;
                    });

                    if (!found)
                    {
                        this.Write("return {jump:2}");    
                    }
                    else
                    {
                        this.Write("break");
                    }
                }
                else
                {
                    this.Write("break");
                }
            }

            this.WriteSemiColon();
            this.WriteNewLine();
        }
    }
}
