document.getElementById('uploadForm').addEventListener('submit', function (e) {
    e.preventDefault();

    const fileInput = document.getElementById('fileInput');
    const file = fileInput.files[0];

    if (!file) {
        showError('Por favor, selecione um arquivo.');
        return;
    }

    const extensoesPermitidas = ['.csv', '.xlsx', '.sql'];
    const fileExtension = file.name.substring(file.name.lastIndexOf('.')).toLowerCase();

    if (!extensoesPermitidas.includes(fileExtension)) {
        showError('Formato de arquivo inválido. Por favor, selecione um arquivo CSV, xlsx ou SQL.');
        return;
    }

    const formData = new FormData();
    formData.append('file', file);


    let url;

    if (fileExtension === '.csv' || fileExtension === '.xlsx') {
        url = 'https://localhost:7017/api/Upload/UploadCsv';
    } else if (fileExtension === '.sql') {
        url = 'https://localhost:7017/api/Upload/UploadSql';
    }



    fetch(url, {
        method: 'POST',
        body: formData,
    })
        .then(response => {
            if (!response.ok) {
                if (response.status === 401) {
                    throw new Error('Você não está autorizado. Faça login novamente.');
                } else if (response.status === 400) {
                    return response.json().then(data => {
                        console.log(data)
                        throw new Error(data.message || 'Erro ao processar o arquivo.');
                    });
                } else if (response.status === 500) {
                    return response.json.then(data => {
                        throw new Error(data.message || "Erro interno no servidor")
                    })
                }   
                else {
                    throw new Error(`Erro na requisição: ${response.statusText}`);
                }
            }
            return response.json();
        })
        .then(data => {
            if (data.success) {
                showSuccess(data.message);
            } else {
                showError(data.message || 'Erro ao processar o arquivo.');
            }
        })
        .catch(error => {
            console.error('Erro:', error);
            showError(error.message || 'Erro ao enviar o arquivo. Tente novamente.');
        });
});

function showError(message) {
    const errorMessage = document.getElementById('fileErrorMessage');
    if (errorMessage) {
        errorMessage.textContent = message;
        errorMessage.style.display = 'block';
        errorMessage.style.color = 'red';
    }
}

function showSuccess(message) {
    const errorMessage = document.getElementById('fileErrorMessage');
    if (errorMessage) {
        errorMessage.style.display = 'none';
    }

    const successMessage = document.getElementById('uploadStatus');
    if (successMessage) {
        successMessage.textContent = message;
        successMessage.style.display = 'block';
        successMessage.style.color = 'green';
    }
}