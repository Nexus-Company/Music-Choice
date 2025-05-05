# **📄 Documentação do Processo de Integração com Múltiplas APIs via Windows Service**

Este documento descreve o processo de autenticação e integração com várias APIs externas de forma simultânea em um **Windows Service**. O sistema foi projetado para ser facilmente extensível, permitindo a adição de novas APIs com mínima configuração.

---

## **🏛 Arquitetura Geral**

O sistema foi projetado para permitir que várias APIs externas sejam integradas e monitoradas simultaneamente por meio de um **Windows Service**. A principal função do sistema é gerenciar a autenticação das APIs e garantir que os tokens de autenticação estejam sempre atualizados. A estrutura é composta pelos seguintes componentes principais:

1. 🔄 **[AuthenticationMonitorWorker](/Solution/Nexus.Music.Choice.Worker/AuthenticationMonitorWorker.cs)**: O worker que monitora todas as APIs e verifica periodicamente o status de autenticação.
2. 🔑 **[IApiAuthenticationService](/Solution/Nexus.Music.Choice.Domain/Services/IApiAuthenticationService.cs)**: Interface que define a lógica de autenticação específica para cada API.
3. 🌐 **[IHttpProvisioningService](/Solution/Nexus.Music.Choice.Domain/Services/IHttpProvisioningService.cs)**: Serviço responsável por abrir uma porta HTTP e ouvir a comunicação da API externa durante o processo de autenticação.
4. 🗄 **[ITokenStoreService](/Solution/Nexus.Music.Choice.Domain/Services/ITokenStoreService.cs)**: Serviço que gerencia os tokens de autenticação, incluindo o armazenamento e recuperação dos tokens.
5. 🚀 **IApiService**: Serviço que consome o **access_token** para fazer requisições à API externa.

Cada componente é modular e pode ser facilmente configurado para integrar novas APIs. **Para integrar uma nova API, não é necessário modificar ou implementar o `AuthenticationMonitorWorker`, mas apenas implementar as interfaces `IApiAuthenticationService` e `IApiService` específicas da nova API.**

---

## **🔐 Importante: Uso de Authorization Code Flow com PKCE**

Para integrações que utilizam o **Authorization Code Flow** (fluxo de código de autorização), recomenda-se fortemente o uso de **PKCE (Proof Key for Code Exchange)**. O PKCE adiciona uma camada de segurança essencial ao fluxo, especialmente em cenários onde a aplicação não pode manter um *client secret* seguro, como em aplicações desktop ou serviços distribuídos. Ele evita ataques de interceptação de código de autorização, garantindo que apenas o cliente legítimo possa trocar o código por tokens.

No contexto deste sistema:  
✅ A implementação de autenticação com PKCE deve ser feita nas classes específicas que implementam `IApiAuthenticationService`.  
✅ Durante o processo de autenticação, a URL gerada para o login do usuário (que inclui os parâmetros PKCE) deve ser **exposta ao ambiente de execução** de forma clara.

> **💡 Recomendação:**  
Configure o sistema para armazenar a URL inicial de login em uma variável de usuário, como:

```powershell
MC_SPOTIFY_LOGIN_URL=https://accounts.spotify.com/authorize?...
```

Essa variável pode ser setada programaticamente ou via configuração no ambiente, facilitando o acesso à URL de login para que os usuários possam concluir a autenticação manualmente ou por meio de outro aplicativo (se necessário).

Para acessar a URL de login você pode seguir o seguinte exemplo de comando:

```powershell
echo $env:MC_SPOTIFY_LOGIN_URL
```

---

## **⚙️ Componentes e Responsabilidades**

### 1. 🔄 **[AuthenticationMonitorWorker](/Solution/Nexus.Music.Choice.Worker/AuthenticationMonitorWorker.cs)**  
Responsável por monitorar todas as APIs registradas. Ele verifica, a cada **1 minuto**, se as autenticações das APIs estão válidas. Quando uma autenticação expira ou se torna inválida, o worker inicia o processo de autenticação para a API correspondente.

**Responsabilidades:**  
- Verificar a autenticação de todas as APIs a cada 1 minuto.  
- Invocar o método `CheckAuthenticationAsync()` de cada implementação de `IApiAuthenticationService` para validar a autenticação.  
- Iniciar o processo de autenticação através do método `StartAuthenticationAsync()` quando necessário.

### 2. 🔑 **[IApiAuthenticationService](/Solution/Nexus.Music.Choice.Domain/Services/IApiAuthenticationService.cs)**  
Define a interface que deve ser implementada para cada API. Cada implementação é responsável pela lógica de autenticação e renovação de tokens de uma API específica.

**Responsabilidades:**  
- `CheckAuthenticationAsync()`: Verifica se a autenticação está válida, retornando `true` se o token for válido. Caso contrário, tenta renovar o token utilizando o **refresh_token**.  
- `StartAuthenticationAsync()`: Inicia o processo de autenticação com a API externa, acionando a comunicação necessária para obter um novo token.

### 3. 🌐 **[IHttpProvisioningService](/Solution/Nexus.Music.Choice.Domain/Services/IHttpProvisioningService.cs)**  
Responsável por gerenciar a comunicação HTTP entre o sistema e a API externa. Ele abre uma porta HTTP e aguarda o evento de autenticação, acionado quando a API externa redireciona o usuário para o serviço.

**Responsabilidades:**  
- Abrir uma porta HTTP para aguardar mensagens de autenticação.  
- Acionar o evento **HttpMessageReceived** quando o código de autenticação é enviado pela API externa.

### 4. 🗄 **[ITokenStoreService](/Solution/Nexus.Music.Choice.Domain/Services/ITokenStoreService.cs)**  
Responsável por armazenar e gerenciar os tokens de autenticação (como **access_token** e **refresh_token**) para cada API.

**Responsabilidades:**  
- `GetTokenAsync()`: Recupera o **access_token** e o **refresh_token** armazenados.  
- `SaveTokenAsync()`: Armazena o **access_token** e o **refresh_token** após a autenticação ser concluída.

> ⚠️ **Nota:** Você não deve armazenar o **access_token**, mas sim o **refresh_token**. A classe abstrata [BaseTokenStoreService](/Solution/Nexus.Music.Choice.Domain/Services/BaseTokenStoreService.cs) já salva de forma segura somente o **refresh_token**.

### 5. 🚀 **IApiService**  
Serviço que consome o **access_token** para fazer requisições autenticadas à API externa. Ele utiliza o **[IApiAuthenticationService](/Solution/Nexus.Music.Choice.Domain/Services/IApiAuthenticationService.cs)** para obter o token e interagir com a API.

**Responsabilidades:**  
- Esperar um novo **access_token** através de um evento `AccessTokenChanged`.  
- Fazer requisições à API externa utilizando o **access_token**.

> 🔒 Por segurança, cada API deve implementar [BaseTokenStoreService](/Solution/Nexus.Music.Choice.Domain/Services/BaseTokenStoreService.cs), garantindo que somente sua implementação tenha acesso aos **access_token**.

---

## **🔁 Fluxo**

O fluxo do processo é projetado para garantir que as autenticações sejam verificadas e renovadas automaticamente. Veja o detalhamento:

1. **Verificação de Autenticação**  
   - O **[AuthenticationMonitorWorker](/Solution/Nexus.Music.Choice.Worker/AuthenticationMonitorWorker.cs)** executa periodicamente, verificando se a autenticação de cada API está válida.  
   - Ele chama o método `CheckAuthenticationAsync()` de cada implementação de `IApiAuthenticationService`.  
   - Se os tokens estiverem válidos, a API é considerada autenticada.  
   - Caso contrário, `StartAuthenticationAsync()` é chamado para renovar a autenticação.

2. **Processo de Autenticação**  
   - O **[IApiAuthenticationService](/Solution/Nexus.Music.Choice.Domain/Services/IApiAuthenticationService.cs)** inicia o processo.  
   - O **[IHttpProvisioningService](/Solution/Nexus.Music.Choice.Domain/Services/IHttpProvisioningService.cs)** abre uma porta HTTP para receber o código de autenticação.  
   - O novo **refresh_token** é armazenado usando `SaveTokenAsync()`.

3. **Renovação de Token**  
   - Se o **access_token** expirar, `CheckAuthenticationAsync()` tenta renová-lo com o **refresh_token**.  
   - Se falhar, o processo completo é reiniciado.

4. **Uso do Token para Requisições**  
   - O **IApiService** acessa o **access_token** via `GetAccessTokenAsync()`.  
   - Realiza as requisições à API externa.

---

## **🛠 Exemplo**

### Integração de Nova API

Para integrar uma nova API, basta implementar as interfaces `IApiAuthenticationService` e `IApiService`. O **[AuthenticationMonitorWorker](/Solution/Nexus.Music.Choice.Worker/AuthenticationMonitorWorker.cs)** já gerencia essas integrações automaticamente.

**Passos:**  
1. **Implementação de `ITokenStoreService`**  
   - Crie uma classe baseada em `BaseTokenStoreService`.  
   - Configure a distribuição do `access_token`.

2. **Implementação de `IApiAuthenticationService`**  
   - Crie uma classe que implemente `IApiAuthenticationService`.  
   - Implemente os métodos `CheckAuthenticationAsync()` e `StartAuthenticationAsync()`.

3. **Implementação de `IApiService`**  
   - Crie uma classe que implemente `IApiService`.  
   - Obtenha e atualize o `access_token` com o `ITokenStoreService`.

4. **Registro no DI**  
   - Registre `IApiService` (Scoped), `IApiAuthenticationService` (Scoped) e `ITokenStoreService` (Singleton).

---

## **✅ Conclusão**

Este sistema foi projetado para permitir a integração e autenticação de múltiplas APIs de maneira simples e eficiente dentro de um **Windows Service**. Ele garante que a autenticação seja sempre válida e que os tokens sejam renovados automaticamente. A modularidade permite expandir facilmente para novas APIs, tornando-o altamente escalável e flexível.
