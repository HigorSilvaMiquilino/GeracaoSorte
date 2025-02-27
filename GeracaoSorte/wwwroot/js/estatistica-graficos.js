document.addEventListener('DOMContentLoaded', async function () {
    try {
        const seriesResponse = await fetch('/api/Estatisticas/serie');
        const seriesJson = await seriesResponse.json();

        if (!seriesJson.success) {
            console.error("Erro ao buscar estatísticas de séries:", seriesJson.message);
            return;
        }

        const seriesData = seriesJson.estatistica;

        const ordensResponse = await fetch('/api/Estatisticas/ordem');
        const ordensJson = await ordensResponse.json();

        if (!ordensJson.success) {
            console.error("Erro ao buscar estatísticas de ordens:", ordensJson.message);
            return;
        }

        const ordensData = ordensJson.estatistica;

        if (!Array.isArray(seriesData) || !Array.isArray(ordensData)) {
            console.error("Dados retornados não são arrays válidos.");
            return;
        }

        seriesData.sort((a, b) => a.quantidade - b.quantidade);
        ordensData.sort((a, b) => a.quantidade - b.quantidade);

        const seriesLabels = seriesData.map(item => item.serie);
        const seriesValues = seriesData.map(item => item.quantidade);

        const ordensLabels = ordensData.map(item => item.serie);
        const ordensValues = ordensData.map(item => item.quantidade);

        const commonOptions = {
            responsive: true,
            maintainAspectRatio: false,
            scales: {
                y: {
                    beginAtZero: true
                }
            }
        };

        new Chart(document.getElementById('serie'), {
            type: 'bar',
            data: {
                labels: seriesLabels,
                datasets: [{
                    label: 'Distribuição de Séries',
                    data: seriesValues,
                    backgroundColor: 'rgba(75, 192, 192, 0.2)',
                    borderColor: 'rgba(75, 192, 192, 1)',
                    borderWidth: 1
                }]
            },
            options: commonOptions
        });

        new Chart(document.getElementById('ordem'), {
            type: 'bar',
            data: {
                labels: ordensLabels,
                datasets: [{
                    label: 'Distribuição de Ordens',
                    data: ordensValues,
                    backgroundColor: 'rgba(153, 102, 255, 0.2)',
                    borderColor: 'rgba(153, 102, 255, 1)',
                    borderWidth: 1
                }]
            },
            options: commonOptions
        });
    } catch (error) {
        console.error("Erro ao carregar dados ou criar gráficos:", error);
    }
});