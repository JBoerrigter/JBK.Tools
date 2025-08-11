using System.Text;

namespace JBK.Tools.ModelLoader.Parsers;

internal enum TokenType { LParen, RParen, Atom, String, EOF }

internal readonly struct Token
{
    public TokenType Type { get; }
    public string Text { get; }
    public Token(TokenType t, string text = "") { Type = t; Text = text ?? ""; }
}

internal sealed class Tokenizer
{
    private readonly string _s;
    private int _i;

    public Tokenizer(string s) { _s = s ?? ""; _i = 0; }

    private void SkipWhitespaceAndComments()
    {
        while (_i < _s.Length)
        {
            char c = _s[_i];
            if (char.IsWhiteSpace(c)) { _i++; continue; }
            if (c == ';') // comment until newline
            {
                while (_i < _s.Length && _s[_i] != '\n') _i++;
                continue;
            }
            break;
        }
    }

    private string ReadQuoted(char quote)
    {
        _i++; // skip open quote
        var sb = new StringBuilder();
        while (_i < _s.Length)
        {
            char c = _s[_i++];
            if (c == quote)
            {
                // doubled quote -> literal quote (original behavior)
                if (_i < _s.Length && _s[_i] == quote) { sb.Append(quote); _i++; continue; }
                return sb.ToString();
            }
            if (c == '\r' || c == '\n') throw new FormatException("Unexpected newline inside quoted string");
            sb.Append(c);
        }
        throw new FormatException("Unterminated quoted string");
    }

    private string ReadSymbol()
    {
        var sb = new StringBuilder();
        while (_i < _s.Length)
        {
            char c = _s[_i];
            if (char.IsWhiteSpace(c) || c == '(' || c == ')' || c == ';') break;
            sb.Append(c); _i++;
        }
        return sb.ToString();
    }

    public Token Next()
    {
        SkipWhitespaceAndComments();
        if (_i >= _s.Length) return new Token(TokenType.EOF);

        char c = _s[_i];
        if (c == '(') { _i++; return new Token(TokenType.LParen); }
        if (c == ')') { _i++; return new Token(TokenType.RParen); }
        if (c == '"' || c == '\'')
        {
            string quoted = ReadQuoted(c);
            return new Token(TokenType.String, quoted);
        }

        string sym = ReadSymbol();
        return new Token(TokenType.Atom, sym);
    }
}