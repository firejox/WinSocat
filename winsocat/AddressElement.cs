namespace APP;

public class AddressElement
{
    private string _tag;
    public string Tag => _tag;
    
    private string _address;
    public string Address => _address;
    
    private Dictionary<string, string> _options;
    public Dictionary<string, string> Options => _options;

    public AddressElement(string tag, string address, Dictionary<string, string> options)
    {
        _tag = tag;
        _address = address;
        _options = options;
    }

    public static AddressElement TryParse(string input)
    {
        string[] tagSplits = input.Split(':', 2);

        if (tagSplits.Length == 1)
        {
            tagSplits = input.Split(',', 2);
            if (tagSplits.Length == 1)
                return new AddressElement(tagSplits[0], "", new Dictionary<string, string>());
            else
                return new AddressElement(tagSplits[0], "", GetOptions(tagSplits[1]));
        }

        int addressSepOffset = GetAddressSepOffset(tagSplits[1]);
        if (addressSepOffset == -1)
            return null!;

        string tag = tagSplits[0];
        string address = tagSplits[1].Substring(0, addressSepOffset);
        var options = GetOptions(tagSplits[1].Substring(addressSepOffset));
        
        return new AddressElement(tag, address, options);
    }
    
    private static int GetAddressSepOffset(string input)
    {
        int length = input.Length;

        var stack = new Stack<char>();

        for (int i = 0; i < length; i++)
        {
            if (input[i] == ',' && stack.Count == 0)
                return i;
            if (input[i] == '\'' || input[i] == '\"')
            {
                if (stack.Count == 0)
                    stack.Push(input[i]);
                else if (stack.Peek() == input[i])
                    stack.Pop();
                else 
                    stack.Push(input[i]);
            }
        }

        if (stack.Count != 0)
            return -1;
        
        return length;
    }

    private static Dictionary<string, string> GetOptions(string input)
    {
        Dictionary<string, string> options = new Dictionary<string, string>();

        string[] optionSplits = input.Split(',',
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var opt in optionSplits)
        {
            int optionSepIndex = opt.IndexOf('=');
            if (optionSepIndex == -1)
                options.Add(opt, "");
            else
                options.Add(opt.Substring(0, optionSepIndex), opt.Substring(optionSepIndex + 1).Trim());
        }

        return options;
    }
}
