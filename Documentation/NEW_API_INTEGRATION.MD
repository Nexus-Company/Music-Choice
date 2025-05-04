# **Documentação do Processo de Integração com Múltiplas APIs via Windows Service**

Este documento descreve o processo de autenticação e integração com várias APIs externas de forma simultânea em um **Windows Service**. O sistema foi projetado para ser facilmente extensível, permitindo a adição de novas APIs com mínima configuração.

---

## **Arquitetura Geral**

O sistema foi projetado para permitir que várias APIs externas sejam integradas e monitoradas simultaneamente por meio de um **Windows Service**. A principal função do sistema é gerenciar a autenticação das APIs e garantir que os tokens de autenticação estejam sempre atualizados. A estrutura é composta pelos seguintes componentes principais:

1. **[AuthenticationMonitorWorker](/Solution/Nexus.Music.Choice.Worker/AuthenticationMonitorWorker.cs)**: O worker que monitora todas as APIs e verifica periodicamente o status de autenticação.
2. **[IApiAuthenticationService](/Solution/Nexus.Music.Choice.Domain/Services/IApiAuthenticationService.cs)**: Interface que define a lógica de autenticação específica para cada API.
3. **[IHttpProvisioningService](/Solution/Nexus.Music.Choice.Domain/Services/IHttpProvisioningService.cs)**: Serviço responsável por abrir uma porta HTTP e ouvir a comunicação da API externa durante o processo de autenticação.
4. **[ITokenService](/Solution/Nexus.Music.Choice.Domain/Services/ITokenStoreService.cs)**: Serviço que gerencia os tokens de autenticação, incluindo o armazenamento e recuperação dos tokens.
5. **IApiService**: Serviço que consome o **access_token** para fazer requisições à API externa.

Cada componente é modular e pode ser facilmente configurado para integrar novas APIs. **Para integrar uma nova API, não é necessário modificar ou implementar o `AuthenticationMonitorWorker`, mas apenas implementar as interfaces `IApiAuthenticationService` e `IApiService` específicas da nova API.**

---

## **Componentes e Responsabilidades**

### 1. **[AuthenticationMonitorWorker](/Solution/Nexus.Music.Choice.Worker/AuthenticationMonitorWorker.cs)**
O **[AuthenticationMonitorWorker](/Solution/Nexus.Music.Choice.Worker/AuthenticationMonitorWorker.cs)** é o responsável por monitorar todas as APIs registradas. Ele verifica, a cada **1 minuto**, se as autenticações das APIs estão válidas. Quando uma autenticação expira ou se torna inválida, o worker inicia o processo de autenticação para a API correspondente.

#### Responsabilidades:
- Verificar a autenticação de todas as APIs a cada 1 minuto.
- Invocar o método `CheckAuthenticationAsync()` de cada implementação de `IApiAuthenticationService` para validar a autenticação.
- Iniciar o processo de autenticação através do método `StartAuthenticationAsync()` quando necessário.

### 2. **[IApiAuthenticationService](/Solution/Nexus.Music.Choice.Domain/Services/IApiAuthenticationService.cs)**
O **[IApiAuthenticationService](/Solution/Nexus.Music.Choice.Domain/Services/IApiAuthenticationService.cs)** define a interface que deve ser implementada para cada API. Cada implementação é responsável pela lógica de autenticação e renovação de tokens de uma API específica.

#### Responsabilidades:
- **CheckAuthenticationAsync()**: Verifica se a autenticação está válida, retornando `true` se o token for válido. Caso contrário, tenta renovar o token utilizando o **refresh_token**.
- **StartAuthenticationAsync()**: Inicia o processo de autenticação com a API externa, acionando a comunicação necessária para obter um novo token.

### 3. **[IHttpProvisioningService](/Solution/Nexus.Music.Choice.Domain/Services/IHttpProvisioningService.cs)**
O **[IHttpProvisioningService](/Solution/Nexus.Music.Choice.Domain/Services/IHttpProvisioningService.cs)** é responsável por gerenciar a comunicação HTTP entre o sistema e a API externa. Ele abre uma porta HTTP e aguarda o evento de autenticação, acionado quando a API externa redireciona o usuário para o serviço.

#### Responsabilidades:
- Abrir uma porta HTTP para aguardar mensagens de autenticação.
- Acionar o evento **HttpMessageReceived** quando o código de autenticação é enviado pela API externa.

### 4. **[ITokenService](/Solution/Nexus.Music.Choice.Domain/Services/ITokenStoreService.cs)**
O **[ITokenService](/Solution/Nexus.Music.Choice.Domain/Services/ITokenStoreService.cs)** é responsável por armazenar e gerenciar os tokens de autenticação (como **access_token** e **refresh_token**) para cada API.

#### Responsabilidades:
- **GetTokenAsync()**: Recupera o **access_token** e o **refresh_token** armazenados.
- **SaveTokenAsync()**: Armazena o **access_token** e o **refresh_token** após a autenticação ser concluída.

### 5. **IApiService**
O **IApiService** é o serviço que consome o **access_token** para fazer requisições autenticadas à API externa. Ele utiliza o **[IApiAuthenticationService](/Solution/Nexus.Music.Choice.Domain/Services/IApiAuthenticationService.cs)** para obter o token e interagir com a API.

#### Responsabilidades:
- Obter o **access_token** através do método `GetAccessTokenAsync()`.
- Fazer requisições à API externa utilizando o **access_token**.

---

## **Fluxo**

O fluxo do processo é projetado para garantir que as autenticações sejam verificadas e renovadas automaticamente. A seguir, o fluxo do processo detalhado:

1. **Verificação de Autenticação**
   - O **[AuthenticationMonitorWorker](/Solution/Nexus.Music.Choice.Worker/AuthenticationMonitorWorker.cs)** executa periodicamente, verificando se a autenticação de cada API está válida.
   - Ele chama o método `CheckAuthenticationAsync()` de cada implementação de `IApiAuthenticationService` para validar a autenticação.
   - Se os tokens estiverem válidos (não expiraram), a API é considerada autenticada e o fluxo continua.
   - Caso os tokens não sejam válidos ou tenham expirado, o método `StartAuthenticationAsync()` é chamado para iniciar o processo de renovação da autenticação.

2. **Processo de Autenticação**
   - Quando a autenticação precisa ser renovada, o **[IApiAuthenticationService](/Solution/Nexus.Music.Choice.Domain/Services/IApiAuthenticationService.cs)** inicia o processo de autenticação.
   - O **[IHttpProvisioningService](/Solution/Nexus.Music.Choice.Domain/Services/IHttpProvisioningService.cs)** abre uma porta HTTP para aguardar a comunicação da API externa. O evento **HttpMessageReceived** é acionado quando a API redireciona o usuário para o serviço de autenticação.
   - Com o código de autenticação fornecido pela URL, o **[IApiAuthenticationService](/Solution/Nexus.Music.Choice.Domain/Services/IApiAuthenticationService.cs)** obtém um novo **access_token** da API externa.
   - O novo **access_token** é armazenado utilizando o método `SaveTokenAsync()` do **[ITokenService](/Solution/Nexus.Music.Choice.Domain/Services/ITokenStoreService.cs)**.

3. **Renovação de Token**
   - Se o **access_token** estiver expirado, o método `CheckAuthenticationAsync()` tentará renová-lo usando o **refresh_token**.
   - Se a renovação falhar, o processo de autenticação é reiniciado.
   - Caso o **refresh_token** também expire ou se torne inválido, o método `StartAuthenticationAsync()` será invocado para tentar obter um novo token.

4. **Uso do Token para Requisições**
   - O **IApiService** pode acessar o **access_token** através do método `GetAccessTokenAsync()`.
   - Usando o **access_token** obtido, o **IApiService** realiza requisições à API externa.

---

## **Exemplo**

### Integração de Nova API

Quando for necessário integrar uma nova API, o único passo requerido é a implementação das interfaces `IApiAuthenticationService` e `IApiService` para a API em questão. O **[AuthenticationMonitorWorker](/Solution/Nexus.Music.Choice.Worker/AuthenticationMonitorWorker.cs)** já está configurado para verificar e monitorar todas as APIs que implementam essas interfaces. Não é necessário implementar ou modificar o **[AuthenticationMonitorWorker](/Solution/Nexus.Music.Choice.Worker/AuthenticationMonitorWorker.cs)**.

#### Passos:
1. **Implementação de `IApiAuthenticationService`**:
   - Crie uma nova classe que implementa a interface `IApiAuthenticationService` e defina a lógica de autenticação específica para a API.
   - Implemente os métodos `CheckAuthenticationAsync()` e `StartAuthenticationAsync()` para gerenciar a autenticação da API.
   
2. **Implementação de `IApiService`**:
   - Crie uma nova classe que implementa a interface `IApiService`.
   - Use o `GetAccessTokenAsync()` do `IApiAuthenticationService` para obter o token e realizar requisições à API externa.

3. **Registro da Nova API**:
   - Registre a implementação de `IApiAuthenticationService` e `IApiService` para que o sistema possa gerenciar a autenticação e realizar requisições.

---

## **Conclusão**

Este sistema foi projetado para permitir a integração e autenticação de múltiplas APIs de maneira simples e eficiente dentro de um **Windows Service**. Ele garante que a autenticação seja sempre válida e que os tokens de autenticação sejam renovados automaticamente quando necessário. O sistema é modular e pode ser facilmente expandido para incluir novas APIs com apenas pequenas modificações, tornando-o altamente escalável e flexível.

---
