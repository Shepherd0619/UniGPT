using UnityEngine;
using TMPro;

public class CodeSnippetHighlighter : MonoBehaviour
{
    [SerializeField] private TMP_Text codeText;

    private void Start()
    {
        FormatAndHighlightCode(codeText.text);
    }
    public void FormatAndHighlightCode(string code)
    {
        // Clear existing text
        codeText.text = "";

        // Split code into lines
        string[] lines = code.Split('\n');

        codeText.text = "<font=\"consola SDF\">";
        // Iterate through each line
        foreach (string line in lines)
        {
            // Split line into words
            string[] words = line.Split(' ');

            // Iterate through each word
            foreach (string word in words)
            {
                // Check if word is a keyword or special syntax
                bool isKeyword = IsKeyword(word);

                // Apply formatting and highlighting
                codeText.text += isKeyword ? "<color=blue>" + word + "</color> " : word + " ";
            }
            codeText.text += "\n";
        }
        codeText.text += "</font>";
    }

    private bool IsKeyword(string word)
    {
        // Add your code snippet keywords here
        string[] keywords = {
        // Common keywords
        "void", "int", "float", "double", "char", "string", "bool",
        "if", "else", "for", "while", "do", "switch", "case", "break", "continue", "return",

        // Java keywords
        "public", "private", "protected", "class", "interface", "extends", "implements", "static",
        "final", "abstract", "super", "this", "new", "instanceof", "package", "import", "throws",
        "throw", "try", "catch", "finally",

        // Python keywords
        "def", "class", "self", "import", "from", "as", "if", "elif", "else", "while", "for", "in",
        "pass", "break", "continue", "return", "is", "not", "and", "or", "try", "except", "finally",
        "raise", "assert", "with", "yield", "lambda",

        // HTML keywords
        "html", "head", "title", "body", "div", "span", "p", "a", "img", "ul", "ol", "li", "table",
        "tr", "td", "th", "form", "input", "button", "select", "option", "style", "script", "link",

        // Visual Basic (VB) keywords
        "dim", "as", "if", "else", "elseif", "then", "for", "next", "do", "while", "loop", "exit",
        "sub", "function", "call", "return", "public", "private", "shared", "new", "class", "inherits",
        "implements", "interface", "property", "get", "set", "select", "case", "is", "not", "and", "or",
        "xor", "not", "true", "false", "const", "module", "namespace", "option", "explicit", "on", "off",

        // C# keywords
        "using", "namespace", "class", "struct", "interface", "enum", "static", "readonly",
        "const", "public", "private", "protected", "internal", "abstract", "sealed", "new",
        "virtual", "override", "base", "this", "true", "false", "null", "delegate", "event",
        "try", "catch", "finally", "throw", "checked", "unchecked", "typeof", "sizeof",
        "default", "async", "await", "lock", "params", "ref", "out", "in", "is", "as",

        // CSS keywords
        "color", "background-color", "font-size", "font-family", "margin", "padding",
        "border", "width", "height", "text-align", "display", "position", "float",
        "top", "bottom", "left", "right", "opacity", "z-index", "box-shadow", "transition"

    };
        // Check if the word is a keyword
        foreach (string keyword in keywords)
        {
            if (word.Equals(keyword))
            {
                return true;
            }
        }

        // If word is not a keyword
        return false;
    }
}
