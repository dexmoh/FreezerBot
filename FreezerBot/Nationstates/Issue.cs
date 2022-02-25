using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nationstates;

public class Issue
{
    public string ID;
    public string Title;
    public string Text;
    public List<(string ID, string Text)> Options;
}

public class IssueAnswer
{
    public string IssueID;
    public string OptionID;
    public string Answer;
}