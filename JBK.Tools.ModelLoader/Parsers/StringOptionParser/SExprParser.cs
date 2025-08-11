namespace JBK.Tools.ModelLoader.Parsers;

public sealed class SExprParser
{
    private readonly Tokenizer _tk;
    private Token _look;

    public SExprParser(string s)
    {
        _tk = new Tokenizer(s);
        _look = _tk.Next();
    }

    private void Consume() => _look = _tk.Next();

    private void Expect(TokenType t)
    {
        if (_look.Type != t) throw new FormatException($"Expected {t} but got {_look.Type}");
        Consume();
    }

    public SExpr Parse()
    {
        // permit multiple top-level items by wrapping into a list
        var items = new List<SExpr>();
        while (_look.Type != TokenType.EOF)
        {
            items.Add(ParseExpr());
        }
        return new SList(items);
    }

    private SExpr ParseExpr()
    {
        if (_look.Type == TokenType.LParen) return ParseList();
        if (_look.Type == TokenType.String)
        {
            var s = _look.Text; Consume();
            return new SAtom(s);
        }
        if (_look.Type == TokenType.Atom)
        {
            var s = _look.Text; Consume();
            return new SAtom(s);
        }
        throw new FormatException($"Unexpected token {_look.Type}");
    }

    private SList ParseList()
    {
        Expect(TokenType.LParen); // consume '('
        var items = new List<SExpr>();
        while (_look.Type != TokenType.RParen)
        {
            if (_look.Type == TokenType.EOF) throw new FormatException("Unterminated list");
            items.Add(ParseExpr());
        }
        Expect(TokenType.RParen); // consume ')'
        return new SList(items);
    }
}