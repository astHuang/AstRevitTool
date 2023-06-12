using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using OfficeOpenXml;
using Microsoft.CodeAnalysis;
using Microsoft.CSharp;
using OpenAI.GPT3;
using OpenAI.GPT3.Managers;
using OpenAI.GPT3.ObjectModels;
using OpenAI.GPT3.ObjectModels.RequestModels;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;


namespace AstRevitTool.Masterclass.Dockable
{
    public class DockablePanelViewModel:ViewModelBase
    {
        public string Input { get; set; }
        public RelayCommand LoadRequirements { get; set; }
        public RelayCommand AddAPIKEY { get; set; }


        private ObservableCollection<RequirementWrapper> _requirements = new ObservableCollection<RequirementWrapper>();
        public ObservableCollection<RequirementWrapper> Requirements
        {
            get { return _requirements; }
            set { _requirements = value; RaisePropertyChanged(() => Requirements); }
        }

        public DockablePanelViewModel()
        {
            LoadRequirements = new RelayCommand(OnLoadRequirements);
            AddAPIKEY = new RelayCommand(OnAddAPIKey);
        }

        public void OnAddAPIKey()
        {
            
            if (Input != string.Empty)
            {
                ChatRevit.Default.GPT_API_KEY = Input;
                ChatRevit.Default.Save();
                MessageBox.Show("API_KEY is " + Input, "Prompt");
                MessageBox.Show("API_KEY Saved Successfully!", "Prompt");
            }
            else
            {
                MessageBox.Show("APK_KEY NOT FOUND", "Prompt");
            }
        }
        private async void OnLoadRequirements()
        {
            if (ChatRevit.Default.GPT_API_KEY == string.Empty)
            {
                MessageBox.Show("Please fill API_KEY first", "Prompt");
                return;
            }
            //MessageBox.Show(WrapPrompt(Input));

            OpenAIService service = new OpenAIService(new OpenAiOptions { ApiKey = ChatRevit.Default.GPT_API_KEY });
            List<ChatMessage> convMessages = new List<ChatMessage>();
            var sysMessage = new ChatMessage("system", systemPersonality);
            var userMessage = new ChatMessage("user", WrapPrompt(Input));
            convMessages.Add(sysMessage);
            convMessages.Add(userMessage);

            var createRequest = new ChatCompletionCreateRequest()
            {
                Messages = convMessages,
                Model = Models.ChatGpt3_5Turbo,
                Temperature = 0.1f
            };

            var res = await service.ChatCompletion.CreateCompletion(createRequest);
            //MessageBox.Show(res.Successful.ToString());
            if (res.Successful)
            {
                var response = res.Choices[0].Message.Content;
                var request = Application.GPTRequestHandler.Raise((app) =>
                {
                    var document = app.ActiveUIDocument.Document;
                    try
                    {
                        using var tran = new Transaction(document, "chat");
                        tran.Start();
                        execute(document, app.ActiveUIDocument, response);                       
                        tran.Commit();
                        MessageBox.Show("Successfully managed your task! \n" +
                            "This is how I solve it: \n" + response
                            ) ;
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message);
                    }
                });
            }
        }

        string systemPersonality => "I want you to act as a Revit Developer. Now Write a Revit C# script of a method.\n" +
                 " - This method provides its functionality as a public function named 'RevitFunction'.\n " +
                 " - This method has only two parameter called 'document' and 'uiDocument' which is passed in from the previous code " +
                 "and the type of parameter 'document' is Document, the type of parameter 'uiDocument' is UIDocument.\n " +
                 " - This method only contains methods available in RevitAPI.\n" +
                 " - This method does not need Transaction.\n" +
                 " - This method immediately does the task when the function is invoked.\n" +
                 " - Don’t use using namesapce and implementation of IExternalCommand.\n" +
                 " - I only need the founction body. Don’t add any explanation.\n";

        string WrapPrompt(string input)
                =>
                 "The task is described as follows:\n" + input;

        private string GetPureCode(string response)
        {
            SyntaxTree tree = CSharpSyntaxTree.ParseText(response);
            CompilationUnitSyntax root = tree.GetCompilationUnitRoot();
            var methods = root.DescendantNodes().OfType<MethodDeclarationSyntax>();
            if (methods.Count() != 0)
            {
                return methods.FirstOrDefault().ToFullString();
            }
            else
            {
                MessageBox.Show("No method found");
                return "";
            }

        }

        private void execute(Document doc, UIDocument uiDoc, string codeOutput)
        {
            var codeString = "using System;\n " +
                             "using System.IO;\n " +
                             "using System.Windows;\n" +
                             "using System.Collections.Generic;\n" +
                             "using System.Linq;\n" +
                             "using Autodesk.Revit.UI;\n" +
                             "using Autodesk.Revit.DB;\n" +
                             "using Autodesk.Revit.DB.Architecture;\n" +
                             "using Autodesk.Revit.ApplicationServices;\n" +
                             "using Autodesk.Revit.UI.Selection;\n"+

                             "namespace Chat {\n" +
                             "public class ChatWithRevit{ \n" + codeOutput + " }\n}\n";

            
            // 创建CodeDom编译器
            CodeDomProvider provider = new CSharpCodeProvider();

            // 定义编译参数
            CompilerParameters parameters = new CompilerParameters();
            parameters.ReferencedAssemblies.Add("System.dll");
            parameters.ReferencedAssemblies.Add("System.Core.dll");
            parameters.ReferencedAssemblies.Add("System.Xml.Linq.dll");
            parameters.ReferencedAssemblies.Add("C:\\Program Files\\Autodesk\\Revit 2020\\RevitAPI.dll");
            parameters.ReferencedAssemblies.Add("C:\\Program Files\\Autodesk\\Revit 2020\\RevitAPIUI.dll");
            parameters.GenerateExecutable = false; // 不生成exe文件
            parameters.GenerateInMemory = true; // 在内存中编译
            parameters.WarningLevel = 4;
            parameters.CompilerOptions = "/target:library";

            // 编译代码
            CompilerResults results = provider.CompileAssemblyFromSource(parameters, codeString);

            // 判断是否有编译错误
            if (results.Errors.HasErrors)
            {
                var errorStr = "Error during compiling：\n";
                foreach (CompilerError error in results.Errors)
                {
                    errorStr += String.Format("Row{0} Column{1}：{2}", error.Line, error.Column, error.ErrorText);
                }
                MessageBox.Show(errorStr);
                return;
            }

            // 获取编译后的程序集
            var assembly = results.CompiledAssembly;

            // 获取实例
            object obj = assembly.CreateInstance("Chat.ChatWithRevit");

            // 执行方法
            obj.GetType().GetMethod("RevitFunction").Invoke(obj, new object[] { doc, uiDoc });
        }

    }
}
