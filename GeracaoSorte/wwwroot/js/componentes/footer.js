export function renderFooter() {
    const footer = document.createElement('footer');
    footer.classList.add('footer');
    footer.innerHTML = `
        <div class="footer-content">
            <p>&copy; 2005 - 2025 - Fenix Systems - Tecnologia em Processamento de Informações LTDA. Todos os direitos reservados.</p>
        </div>
    `;
    return footer;
}
