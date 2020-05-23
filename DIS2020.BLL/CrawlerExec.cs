using DIS2020.DOMAIN.Models;
using Microsoft.Extensions.Configuration;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using System.IO;

namespace DIS2020.BLL
{
    public class CrawlerExec
    {
        private IConfiguration config;
        public CrawlerExec(IConfiguration _config)
        {
            config = _config;
        }

        /// <summary>
        /// Retornar os que irão para o angular.
        /// </summary>
        public DadosPagina ObterDadosDaPagina()
        {
            int porcentagem = 0;
            string data = "";
            string dataDB = "";

            var caminho = Environment.CurrentDirectory;

            using (var driver = new ChromeDriver(caminho))
            {
                driver.Navigate().GoToUrl("https://www.saopaulo.sp.gov.br/coronavirus/");

                var sitePorcentagem = driver.FindElementByClassName("num-isolamento").FindElement(By.ClassName("pnumeros")).Text;
                string[] numbers = Regex.Split(sitePorcentagem, @"\D+");
                porcentagem = Convert.ToInt32(numbers[0]);

                var siteData = driver.FindElementByClassName("num-isolamento").FindElement(By.ClassName("dados-atualizados")).Text;
                string[] listaData = Regex.Split(siteData, @"\D+");
                data = listaData[1] + "/" + listaData[2];
                dataDB = "20" + "0"+listaData[2] + listaData[1];
            }

            var resultado = new DadosPagina{
                taxaIsolamento = porcentagem,
                dataTaxa = data,
                dataBanco =  dataDB
            };

            return resultado;
        }
        
        /// <summary>
        /// Verifica se a data obtida no site já foi registrada no banco de dados.
        /// Retorna TRUE para caso já exista o dado na tabela.
        /// </summary>
        public bool VerificaDataMedicao(DadosPagina dados)
        {
            var param = PrepParamGet(dados.dataBanco, "dia");

            var svc = new BaseService(config);
            var res = svc.ExecuteProcGet<TbTaxa>("STP_DIS2020_TAXA_GET_OPS", param);
            
            if(res.Count() > 0)
            {
                return true;
            } else
            {
                return false;
            }
        }

        /// <summary>
        /// Retorna todos os dados da tabela
        /// </summary>
        /// <returns></returns>
        public IEnumerable<TbTaxa> TrazerTodosDados()
        {
            var param = PrepParamGet("", "all");

            var svc = new BaseService(config);
            var res = svc.ExecuteProcGet<TbTaxa>("STP_DIS2020_TAXA_GET_OPS", param);

            return res;
        }

        /// <summary>
        /// Caso o DB não tenha salvo este dia de medição, esse método irá atualizá-lo
        /// </summary>
        public void AtualizaDBMedicaoAtual(DadosPagina dados)
        {
            var param = PrepParamPost(dados.taxaIsolamento, dados.dataBanco);

            var svc = new BaseService(config);
            var res = svc.ExecuteProc("STP_DIS2020_TAXA_OPS", param);

        }

        private Dictionary<string, object> PrepParamGet(string dia,  string operation)
        {
            var param = new Dictionary<string, object>
            {
                { "@dia", dia },
                { "@ops", operation }
            };

            return param;
        }

        private Dictionary<string, object> PrepParamPost(int taxa, string data)
        {
            var param = new Dictionary<string, object>
            {
                { "@taxa_entry", taxa },
                { "@data_entry", data }
            };

            return param;
        }
    }
}
