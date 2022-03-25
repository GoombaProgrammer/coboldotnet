using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using Microsoft.VisualBasic;
using System.Windows.Forms;
using System.IO;
using Microsoft.CSharp;
using System.CodeDom.Compiler;

namespace COBOL.NET
{
    public partial class ProjectForm : Form
    {
        public string ProjectName = "Untitled";
        public string SelectedItem = "";
        public string CSDefault = "using System;\nusing System.Collections.Generic;\nusing System.Linq;\nusing System.Text;\n\nnamespace ConsoleApp4\n    {\n        class Program\n        {\n";
        public string ParsedCSCode = "";

        public ProjectForm()
        {
            InitializeComponent();
        }

        private void ProjectForm_Load(object sender, EventArgs e)
        {
            this.Text = ProjectName + " - Super COBOL.NET";
            this.saveCodeToolStripMenuItem.Enabled = false;
            ParsedCSCode = CSDefault;
            Refreshlistbox();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string input = Interaction.InputBox("New File Name", "Create project file");
            if (input != "")
            {
                System.IO.File.Create(ProjectName + "\\" + input).Close();
            }
            Refreshlistbox();
        }
        void Refreshlistbox()
        {
            listBox1.Items.Clear();
            string filepath = ProjectName;
            DirectoryInfo d = new DirectoryInfo(filepath);

            foreach (var file in d.GetFiles("*.*"))
            {
                listBox1.Items.Add(file);
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void listBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (listBox1.SelectedIndex > -1)
            {
                SelectedItem = listBox1.SelectedItem.ToString();
                fastColoredTextBox1.Text = File.ReadAllText(ProjectName + "\\" + SelectedItem);
                fastColoredTextBox1.ReadOnly = false;
                this.saveCodeToolStripMenuItem.Enabled = true;
            }
        }

        private void closeCodeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            fastColoredTextBox1.Text = "";
            fastColoredTextBox1.ReadOnly = true;
            this.saveCodeToolStripMenuItem.Enabled = false;
            this.compileToolStripMenuItem.Enabled = false;
            SelectedItem = "";
        }

        private void saveCodeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            File.WriteAllText(ProjectName + "\\" + SelectedItem, fastColoredTextBox1.Text);
        }

        private void compileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listBox2.Items.Clear();
            ParsedCSCode = CSDefault;
            if (ParseCode(fastColoredTextBox1.Text))
            {
                MessageBox.Show(ParsedCSCode);
                CompileCSharpCode(ParsedCSCode, ProjectName + "\\" + "out.exe");
            }
            Refreshlistbox();

        }
        public bool CompileCSharpCode(string sourceFile, string exeFile)
        {
            CSharpCodeProvider provider = new CSharpCodeProvider();
            CompilerParameters cp = new CompilerParameters();
            cp.ReferencedAssemblies.Add("System.dll");
            cp.ReferencedAssemblies.Add("System.Core.dll");
            cp.ReferencedAssemblies.Add("System.Data.dll");
            cp.ReferencedAssemblies.Add("System.Xml.dll");
            cp.ReferencedAssemblies.Add("System.Xml.Linq.dll");
            cp.GenerateExecutable = true;
            cp.OutputAssembly = exeFile;
            cp.GenerateInMemory = false;
            CompilerResults cr = provider.CompileAssemblyFromSource(cp, sourceFile);
            if (cr.Errors.Count > 0)
            {
                listBox2.Items.Add("Errors building source");
                foreach (CompilerError ce in cr.Errors)
                {
                    MessageBox.Show(ce.ToString());
                }
            }
            if (cr.Errors.Count > 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        bool ParseCode(string code, bool include = false)
        {
            bool ERRORED = false;
            int i = 0;
            using (StringReader reader = new StringReader(code))
            {
                string line = string.Empty;
                do
                {
                    line = reader.ReadLine();
                    if (line != null)
                    {
                        i++;
                        line = line.Replace("\t", "");
                        while (line.IndexOf("  ") >= 0)
                        {
                            line = line.Replace("  ", "");
                        }
                        if (line.StartsWith("++INCLUDE"))
                        {
                            if (line.Split(' ').Length > 1)
                            {
                                if (File.Exists(ProjectName + "\\" + line.Split(' ')[1]))
                                {
                                    ParseCode(File.ReadAllText(ProjectName + "\\" + line.Split(' ')[1]), true);
                                } else
                                {
                                    listBox2.Items.Add("Include not found at line " + i);
                                    ERRORED = true;
                                }
                            } else
                            {
                                listBox2.Items.Add("Cannot include a NULL value at line " + i);
                                ERRORED = true;
                            }
                        }
                        if (line.StartsWith("DISPLAY"))
                        {
                            if (line.Split(' ').Length > 1)
                            {
                                if (line.Substring(7).StartsWith("\""))
                                {
                                    if (line.Split('"')[2] != ".")
                                    {
                                        listBox2.Items.Add("Missing . at line " + i);
                                        ERRORED = true;
                                    }
                                    ParsedCSCode += "Console.WriteLine(\"" + line.Split('"')[1] + "\");\n";
                                }
                                else
                                {
                                    if (line.Split(line.Split(' ')[1].Replace(".", "").ToCharArray())[1] == ".")
                                    {
                                        listBox2.Items.Add("Missing . at line " + i);
                                        ERRORED = true;
                                    }
                                    ParsedCSCode += "Console.WriteLine(" + line.Substring(7).Remove(line.Substring(7).Length - 1) + ");\n";
                                }
                            } else
                            {
                                listBox2.Items.Add("DISPLAY syntax error at line " + i);
                                ERRORED = true;
                            }
                        }
                        if (line.StartsWith("STOP RUN."))
                        {
                            ParsedCSCode += "Environment.Exit(0);\n";
                        }
                        if (line.Split(' ')[0] == "STOP")
                        {
                            if (line.Split(' ')[1] == "RUN")
                            {
                                listBox2.Items.Add("Missing . at line " + i);
                                ERRORED = true;
                            }
                        }
                        if (line.StartsWith("END"))
                        {
                            ParsedCSCode += "}\n";
                        }
                        if (line.StartsWith("IF"))
                        {
                            if (line.Contains("THEN"))
                            {
                                var Condition = line.Substring(2, line.IndexOf("THEN") - 2);
                                ParsedCSCode += "if (" + Condition + ")\n{\n";
                            } else
                            {
                                listBox2.Items.Add("Missing THEN at line " + i);
                                ERRORED = true;
                            }
                        }
                        if (line.StartsWith("ELSE"))
                        {
                            ParsedCSCode += "}\nelse\n{\n";
                        }
                        if (line.StartsWith("CALL"))
                        {
                            if (line.Split(' ').Length > 1)
                            {
                                ParsedCSCode += line.Split(' ')[1] + "(" + line.Substring(4 + line.Split(' ')[1].Length) + ");\n";
                            }
                        }
                        if (line.StartsWith("PROCEDURE"))
                        {
                            if (line.Split(' ').Length > 1)
                            {
                                if(line.Substring(9) == " DIVISION" || line.Substring(9) == " MAIN")
                                {
                                    ParsedCSCode += "static void Main()\n{\n";
                                }
                                else
                                {
                                    ParsedCSCode += "static void " + line.Substring(9) + "\n{\n";
                                }
                            }
                        }
                        if (line.StartsWith("STRING-PROCEDURE"))
                        {
                            if (line.Split(' ').Length > 1)
                            {
                                if (line.Substring(16) == " DIVISION" || line.Substring(16) == " MAIN")
                                {
                                    ParsedCSCode += "static string Main()\n{\n";
                                }
                                else
                                {
                                    ParsedCSCode += "static string " + line.Substring(16) + "\n{\n";
                                }
                            }
                        }
                        if (line.StartsWith("NUMBER-PROCEDURE"))
                        {
                            if (line.Split(' ').Length > 1)
                            {
                                if (line.Substring(16) == " DIVISION" || line.Substring(16) == " MAIN")
                                {
                                    ParsedCSCode += "static int Main()\n{\n";
                                }
                                else
                                {
                                    ParsedCSCode += "static int " + line.Substring(16) + "\n{\n";
                                }
                            }
                        }
                        if (line.StartsWith("MOVE"))
                        {
                            if (line.Split(' ').Length > 2)
                            {
                                if (line.Split(' ')[1].StartsWith("\""))
                                {
                                    if (line.Split('"')[2].StartsWith(" TO"))
                                    {
                                        if (line.Split('"')[2].EndsWith("."))
                                        {
                                            ParsedCSCode += line.Split('"')[2].Replace(".", "").Replace(" TO", "") + " = " + "\"" + line.Split('"')[1] + "\"" + ";\n";
                                        }
                                        else
                                        {
                                            listBox2.Items.Add("Missing . at line " + i);
                                            ERRORED = true;
                                        }
                                    }
                                    else
                                    {
                                        listBox2.Items.Add("MOVE syntax error at line " + i);
                                        ERRORED = true;
                                    }
                                }
                                else
                                {
                                    if (line.Split(' ')[2] == "TO")
                                    {
                                        if (line.Split(' ')[3].EndsWith("."))
                                        {
                                            ParsedCSCode += line.Split(' ')[3].Replace(".", "") + " = " + line.Split(' ')[1] + ";\n";
                                        }
                                        else
                                        {
                                            listBox2.Items.Add("Missing . at line " + i);
                                            ERRORED = true;
                                        }
                                    }
                                    else
                                    {
                                        listBox2.Items.Add("MOVE syntax error at line " + i);
                                        ERRORED = true;
                                    }
                                }
                            } else
                            {
                                listBox2.Items.Add("MOVE syntax error at line " + i);
                                ERRORED = true;
                            }
                        }
                        if (line.StartsWith("ACCEPT"))
                        {
                            if (line.EndsWith("."))
                            {
                                if (line.Split(' ')[2] == "FROM")
                                {
                                    if (line.Split(' ')[3] == "SYSIN.")
                                    {
                                        ParsedCSCode += line.Split(' ')[1].Replace(".", "") + " = Console.ReadLine();\n";
                                    }
                                } else
                                {
                                    listBox2.Items.Add("ACCEPT syntax error at line " + i);
                                    ERRORED = true;
                                }
                            } else
                            {
                                listBox2.Items.Add("Missing . at line " + i);
                                ERRORED = true;
                            }
                        }
                        if (line.StartsWith("RETURN"))
                        {
                            if (line.EndsWith("."))
                            {
                                if (line.Split(' ').Length > 1)
                                {
                                    ParsedCSCode += "return " + line.Split(' ')[1].Replace(".","") + ";\n";
                                }
                            }
                            else
                            {
                                listBox2.Items.Add("Missing . at line " + i);
                                ERRORED = true;
                            }
                        }
                        if (line.Split(' ')[0] == "STRING")
                        {
                            if (line.EndsWith("."))
                            {
                                if (line.Split(' ').Length > 2)
                                {
                                    ParsedCSCode += line.Split(' ')[1] + " = ";
                                    for (var X = 2; X < line.Split(' ').Length; X++)
                                    {
                                        if (X == line.Split(' ').Length - 1)
                                        {
                                            ParsedCSCode += line.Split(' ')[X].Replace(".", "")  + ";\n";
                                        }
                                        else
                                        {
                                            ParsedCSCode += line.Split(' ')[X].Replace(".", "") + " + ";
                                        }
                                    }
                                } else
                                {
                                    listBox2.Items.Add("STRING syntax error at line " + i);
                                    ERRORED = true;
                                }
                            }
                            else
                            {
                                listBox2.Items.Add("Missing . at line " + i);
                                ERRORED = true;
                            }
                        }
                        if (line.Split(' ').Length > 1)
                        {
                            if (line.Split(' ')[1] == "VAR.")
                            {
                                if (line.Split(' ').Length < 2)
                                {
                                    listBox2.Items.Add("Missing . at line " + i);
                                    ERRORED = true;
                                }
                                ParsedCSCode += "string " + line.Split(' ')[0].Replace(".", "") + " = \"\";\n";
                            }
                            if (line.Split(' ')[1] == "NUMBER-VAR.")
                            {
                                if (line.Split(' ').Length < 2)
                                {
                                    listBox2.Items.Add("Missing . at line " + i);
                                    ERRORED = true;
                                }
                                ParsedCSCode += "int " + line.Split(' ')[0].Replace(".", "") + " = 0;\n";
                            }
                            if (line.Split(' ')[1] == "VAR")
                            {
                                listBox2.Items.Add("Missing . at line " + i);
                                ERRORED = true;
                            }
                        }
                    }

                } while (line != null);
                if (!include)
                {
                    ParsedCSCode += "\n        }\n        }";
                }
            }
            if (ERRORED)
            {
                return false;
            }
            return true;
        }
    }
}
