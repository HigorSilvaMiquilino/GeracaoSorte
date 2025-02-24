export function renderHeader() {
    const header = document.createElement('header');

    header.innerHTML = `
        <nav class="navbar">
            <div class="container">
                <a href="#" class="logo">
                    <img src="/assets/imagens/logo.png" alt="Fenix System">
                </a>
                <button class="navbar-toggle" aria-label="Abrir menu">
                    <span></span>
                    <span></span>
                    <span></span>
                </button>
                <ul class="navbar-menu">
                    <li><a href="/html/index.html" class="active">Início</a></li>
                    <li><a href="/html/index.html">Funcionalidades</a></li>
                    <li><a href="/html/carregar.html">Carregar</a></li>
                    <li><a href="/html/lista-clientes.html">Lista de clientes</a></li>
                </ul>
            </div>
        </nav>
    `;
    return header;
}