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
            //Spider atual = new Spider();
            Spider thr01 = new Spider("tid01");
            Thread tid01 = new Thread(new ThreadStart(thr01.Inicia));

            Spider thr02 = new Spider("tid02");
            Spider thr03 = new Spider("tid03");
            Spider thr04 = new Spider("tid04");
            Spider thr05 = new Spider("tid05");
            Spider thr06 = new Spider("tid06");
            Spider thr07 = new Spider("tid07");
            Spider thr08 = new Spider("tid08");
            Spider thr09 = new Spider("tid09");
            Spider thr10 = new Spider("tid10");

            
            Thread tid02 = new Thread(new ThreadStart(thr02.Inicia)); //tid02.Name = "tid02";
            Thread tid03 = new Thread(new ThreadStart(thr03.Inicia)); //tid03.Name = "tid03";
            Thread tid04 = new Thread(new ThreadStart(thr04.Inicia)); //tid04.Name = "tid04";
            Thread tid05 = new Thread(new ThreadStart(thr05.Inicia)); //tid05.Name = "tid05";
            Thread tid06 = new Thread(new ThreadStart(thr06.Inicia)); //tid06.Name = "tid06";
            Thread tid07 = new Thread(new ThreadStart(thr07.Inicia)); //tid07.Name = "tid07";
            Thread tid08 = new Thread(new ThreadStart(thr08.Inicia)); //tid08.Name = "tid08";
            Thread tid09 = new Thread(new ThreadStart(thr09.Inicia)); //tid09.Name = "tid09";
            Thread tid10 = new Thread(new ThreadStart(thr10.Inicia)); //tid10.Name = "tid10";

            Spider thr11 = new Spider("tid11");
            Thread tid11 = new Thread(new ThreadStart(thr11.Inicia));
            Spider thr12 = new Spider("tid12");
            Thread tid12 = new Thread(new ThreadStart(thr12.Inicia));
            Spider thr13 = new Spider("tid13");
            Thread tid13 = new Thread(new ThreadStart(thr13.Inicia));
            Spider thr14 = new Spider("tid14");
            Thread tid14 = new Thread(new ThreadStart(thr14.Inicia));
            Spider thr15 = new Spider("tid15");
            Thread tid15 = new Thread(new ThreadStart(thr15.Inicia));
            Spider thr16 = new Spider("tid16");
            Thread tid16 = new Thread(new ThreadStart(thr16.Inicia));
            Spider thr17 = new Spider("tid17");
            Thread tid17 = new Thread(new ThreadStart(thr17.Inicia));
            Spider thr18 = new Spider("tid18");
            Thread tid18 = new Thread(new ThreadStart(thr18.Inicia));
            Spider thr19 = new Spider("tid19");
            Thread tid19 = new Thread(new ThreadStart(thr19.Inicia));

            tid01.Start();
            tid02.Start();
            tid03.Start();
            tid04.Start();
            tid05.Start();
            tid06.Start();
            tid07.Start();
            tid08.Start();
            tid09.Start();
            tid10.Start();
            tid11.Start();
            tid12.Start();
            tid13.Start();
            tid14.Start();
            tid15.Start();
            tid16.Start();
            tid17.Start();
            tid18.Start();
            tid19.Start();

            //atual.Inicia();

            var tempoFinal = DateTime.Now;

            Console.WriteLine("De " + tempoInicial.ToShortTimeString());
        }

    }

    public class Spider
    {
        private ReaderWriterLockSlim countLock = new ReaderWriterLockSlim();

        public static ListaUrls listaPrincipal = new ListaUrls();
        public static int Cached;
        public string root;
        private string currentThreadName;

        public Spider(string name)
        {
            root = "http://memoria.petrobras.com.br/";
            currentThreadName = name;
            Console.WriteLine(currentThreadName);
        }

        public void Inicia()
        {
            
            AcessarLinks(root);
        }

        public void PopulaListaPrincipal(string link)
        {
            if (!link.Contains("http"))
            {
                link = root + link;
                link = link.Replace("//", "/");
                link = link.Replace("http:/", "http://");
            }
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

                    if (!novo.Contains("#")
                        && !novo.Contains("javascript:void(0);")
                        && !string.IsNullOrWhiteSpace(novo)
                        && novo != "/"
                        && !novo.Contains("http")
                        && !listaPrincipal.Contains(novo)
                        && novo[novo.Length - 4] != '.')
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

        public void AcessarLinks(string url)
        {
            while (!string.IsNullOrWhiteSpace(url))
            {
                Console.WriteLine(currentThreadName + ": (" + Cached +"/"+listaPrincipal.Count() + ") " + url);
                PopulaListaPrincipal(url);
                url = listaPrincipal.ReadNext();
            }
            Console.WriteLine("Até " + DateTime.Now.ToShortTimeString());
        }
    }

}