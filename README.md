# Documentação do Projeto WorkerService

## **Visão Geral**
O projeto **WorkerService** é um serviço backend que integra monitoramento de arquivos com automação web utilizando Selenium WebDriver.  
Seu objetivo é processar comandos recebidos em um arquivo de configuração (`reprocess.txt`), buscar informações em sites utilizando Selenium, padronizar e salvar os resultados em formato JSON e enviá-los para um servidor SFTP.

---

## **Funcionalidades**
- **Monitoramento de Arquivos**: Monitora o arquivo `reprocess.txt` para detectar comandos de reprocessamento.
- **Automação com Selenium**: Realiza buscas automatizadas no site especificado, extraindo informações detalhadas.
- **Geração de JSON**: Padroniza os resultados da busca e salva em arquivos JSON.
- **Envio via SFTP**: Transfere os arquivos JSON gerados para um servidor remoto.
- **Configuração Flexível**: Permite personalização de parâmetros como modelo de veículo, tempos de execução e caminhos de arquivos por meio do arquivo `config.json`.

---

## **Configuração do Arquivo `config.json`**
O arquivo `config.json` permite ajustes intuitivos para personalizar a execução do serviço.  
Por meio dele, é possível:
- Alterar o site e o veículo pesquisado.
- Configurar os elementos HTML a serem buscados (como **xPath**, **tags**, **classes**, e **ids**).  
Esse mapeamento pode ser feito inspecionando o site de destino.

Exemplo de um `config.json` bem configurado:
```json
{
  "Driver": "chrome",
  "Url": "https://www.chavesnamao.com.br/carros/brasil/",
  "CarToBeSearched": "Ferrari",
  "SearchFieldId": "inpSearch",
  "ResultBlock": ".block",
  "ResultFieldSelector": "//div[@class=\"veiculos__Card-sc-3pfc6p-0 igtaVV   \"]",
  "CarInfoSelectors": {
    "Name": "h2 > span",
    "Description": "h2 > small",
    "Price": "p.price > b",
    "Year": "ul.list > li",
    "Origin": "h3"
  },
  "SftpConfig": {
    "Protocol": "Sftp",
    "SshHostKeyFingerprint": "ssh-rsa 2048 oAlOFUsNm0XpPq9oTk+2xQF+h5B/zmjWLKyleBgofC8",
    "Server": "192.168.56.1",
    "Port": 3222,
    "User": "luizarruda",
    "Password": "luizarruda"
  },
  "FilePaths": {
    "LocalFilesPath": "C:\\Users\\luiz.arruda\\source\\repos\\WorkerService\\WorkerService\\bin\\Debug\\net8.0\\archivesJson",
    "RemoteFilesPath": "/"
  },
  "DelayOfWorkers": {
    "SeleniumWorkerDelay": 90000,
    "SftpWorkerDelay": 120000
  }
}
```

---

## **Configuração do Arquivo `nlog.config`**

O arquivo `nlog.config` é utilizado para configurar o registro de logs do projeto. Ele permite definir o formato, o nível de log e o local onde os arquivos de log serão armazenados. É essencial configurar corretamente a pasta de destino para garantir que os logs sejam salvos no local desejado.

### **Exemplo de Estrutura do `nlog.config`**
```
<nlog xmlns="http://www.nlog-project.org/schemas/NLog.xsd"
      xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
      autoReload="true"
      internalLogFile="C:\temp\nlog-internal.log"
      internalLogLevel="Debug">

	<targets>
		<target xsi:type="File" name="logfile"
                fileName="C:\Users\luiz.arruda\source\repos\WorkerService\WorkerService\bin\Debug\net8.0\logs\log.log"
                layout="${longdate}|${level}|${message}"
                createDirs="true" />

		<target xsi:type="File" name="logError"
                fileName="C:\Users\luiz.arruda\source\repos\WorkerService\WorkerService\bin\Debug\net8.0\logs\logError.log"
                layout="${longdate}|${level}|${message}"
                createDirs="true" />
	</targets>

	<rules>
		<logger name="*" minlevel="Info" maxlevel="Warn" writeTo="logfile" />
		<logger name="*" minlevel="Error" writeTo="logError" />
	</rules>
</nlog>
```

---

## **Tecnologias Utilizadas**
### **Backend**
- **.NET Core 6.0**: Framework principal para o desenvolvimento do serviço.
- **Selenium WebDriver**: Biblioteca para automação de tarefas no navegador.
- **NLog**: Ferramenta de logging para monitorar o desempenho e erros no sistema.

### **Infraestrutura**
- **SFTP**: Protocolo para transferência segura dos arquivos gerados.
- **JSON**: Formato utilizado para padronizar os dados extraídos.

---

## **Instalação e Execução**
1. **Clone o repositório:**
   ```bash
   https://github.com/luizarrudaz/WorkerService.git
   ```
2. **Restaure as dependências:**
   ```bash
   dotnet restore
   ```
3. **Compile e execute:**
   ```bash
   dotnet run
   ```
