using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;

namespace Robotinic_2._0
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var tempoInicial = DateTime.Now;
            Task[] taskArray = new Task[10];

            for (int i = 0; i < taskArray.Length; i++)
            {
                taskArray[i] = new Task((obj) =>
                {
                    Spider aranha = (Spider)obj;
                    aranha.Inicia();
                }, new Spider("tid"+i){});
                taskArray[i].Start();
            }

            Task.WaitAll(taskArray);

            var tempoFinal = DateTime.Now;

            Console.WriteLine("De " + tempoInicial.ToShortTimeString());
            Console.WriteLine("Tempo Inicial: " + tempoInicial);
            Console.WriteLine("Tempo Final: " + tempoFinal);
            Console.ReadLine();
            Console.ReadKey();
        }

    }

    public class Spider
    {
        private ReaderWriterLockSlim countLock = new ReaderWriterLockSlim();

        public static ListaUrls listaPrincipal = new ListaUrls();
        public static int Cached;
        private string root;
        private string currentThreadName;

        public Spider(string name)
        {
            root = "http://memoria.petrobras.com.br/";
            currentThreadName = name;
            Console.WriteLine(currentThreadName);
        }

        public void Inicia()
        {
            string url = string.Empty;

            if (!listaPrincipal.Contains(root))
            {
                listaPrincipal.Add(root, true);
                PopulaListaPrincipal(root);
            }
            else
            {
                // Espera a primeira carga da lista
                Thread.Sleep(60000);
            }
            
            url = listaPrincipal.ReadNext();

            while (!string.IsNullOrWhiteSpace(url))
            {
                Console.WriteLine(currentThreadName + ": (" + Cached + "/" + listaPrincipal.Count() + ") " + url);
                PopulaListaPrincipal(url);
                url = listaPrincipal.ReadNext();
            }
            Console.WriteLine(currentThreadName + " terminou em " + DateTime.Now.ToShortTimeString());
        }

        public void PopulaListaPrincipal(string link)
        {
            
            try
            {
                #region FazRequestUrl
                WebRequest requisicao = WebRequest.Create(link);
                requisicao.Timeout = 1200000;
                Stream resposta = requisicao.GetResponse().GetResponseStream();
                StreamReader sr = new StreamReader(resposta);

                try
                {
                    countLock.EnterWriteLock();
                    Cached++;
                }
                finally
                {
                    countLock.ExitWriteLock();
                }

                #endregion


                #region PegaLinksDoConteudo
                Match m;
                string HRefPattern = "href\\s*=\\s*(?:[\"'](?<1>[^\"']*)[\"']|(?<1>\\S+))";


                m = Regex.Match(sr.ReadToEnd(), HRefPattern,
                                RegexOptions.IgnoreCase | RegexOptions.Compiled,
                                TimeSpan.FromSeconds(1));
                while (m.Success)
                {
                    string novo = m.Groups[1].ToString().ToLower();

                    if (novo.Contains('#'))
                        novo = novo.Split('#')[0];
                    if (!novo.Contains("http"))
                    {
                        novo = root + novo;
                    }

                    novo = novo.Replace("//", "/");
                    novo = novo.Replace("http:/", "http://");

                    if (!novo.Contains("void(0)")
                        && !listaPrincipal.Contains(novo)
                        && novo[novo.Length - 4] != '.'
                        && novo[novo.Length - 3] != '.'
                        && novo.Contains(root)
                        && novo.Length > root.Length+2)
                    { 
                        while (!listaPrincipal.AddWithTimeout(novo, false, 2000)) { };
                    }
                    m = m.NextMatch();
                }

                #endregion
            }
            catch
            {
                Console.WriteLine("Erro: " + link);
            }
        }

     
        public void ResetarIIS()
        {
            System.Diagnostics.Process P = new System.Diagnostics.Process();

            P.StartInfo.FileName = Environment.GetFolderPath(Environment.SpecialFolder.System) + @"\iisreset.exe";
            P.Start();
            Console.ReadLine();
        }
    }
}