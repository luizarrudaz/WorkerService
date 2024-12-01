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
   git clone https://github.com/luizarrudas/WorkerService.git
   ```
2. **Restaure as dependências:**
   ```bash
   dotnet restore
   ```
3. **Compile e execute:**
   ```bash
   dotnet run
   ```
