using TMPro;
using UnityEngine;

public class CodeSnippetHighlighter : MonoBehaviour
{
    public void FormatAndHighlightCode(TMP_Text codeText)
    {
        // Get the existing text
        string existingText = codeText.text;

        // Get the start and end delimiter for the code block
        string delimiter = "```";

        // Find the start and end indices of the code block
        int startIndex = existingText.IndexOf(delimiter);
        int endIndex = existingText.LastIndexOf(delimiter);

        // Check if the code block delimiters are found
        if (startIndex != -1 && endIndex != -1)
        {
            // Extract the code block
            string codeBlock = existingText.Substring(startIndex + delimiter.Length, endIndex - startIndex - delimiter.Length).Trim();

            // Split the code block into lines
            string[] codeLines = codeBlock.Split('\n');

            string result = "<font=\"consola SDF\">";
            // Iterate through each line in the code block
            foreach (string line in codeLines)
            {
                // Split the line into words
                string[] words = line.Split(' ');

                // Iterate through each word
                foreach (string word in words)
                {
                    // Check if the word is a keyword or special syntax
                    bool isKeyword = IsKeyword(word);

                    // Apply formatting and highlighting
                    result += isKeyword ? "<color=blue>" + word + "</color> " : word + " ";
                }
                result += "\n";
            }
            result += "</font>";

            existingText = existingText.Replace(existingText.Substring(startIndex, endIndex - startIndex + delimiter.Length), result);

            codeText.text = existingText;
        }
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
        "top", "bottom", "left", "right", "opacity", "z-index", "box-shadow", "transition",

        // PHP keywords
        "echo", "if", "else", "elseif", "endif", "while", "do", "for", "foreach", "as",
        "break", "continue", "switch", "case", "default", "function", "return", "var",
        "const", "class", "abstract", "interface", "trait", "namespace", "use", "require",
        "require_once", "include", "include_once", "throw", "try", "catch", "finally",
        "global", "static", "public", "private", "protected", "final", "extends", "implements",
        "__DIR__", "__FILE__", "__LINE__", "__FUNCTION__", "__CLASS__", "__METHOD__"

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
