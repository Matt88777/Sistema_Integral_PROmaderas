// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
// Buscar productos (autosuggest)
function buscarProductos(q, onSuccess, onError) {
    if (!q || q.length < 2) {
        onSuccess([]);
        return;
    }

    $.ajax({
        url: '/api/productos/buscar',
        type: 'GET',
        data: { q: q },
        dataType: 'json',
        success: function (data) {
            onSuccess(data);
        },
        error: function (xhr) {
            if (onError) onError(xhr);
        }
    });
}

// Calcular totales del pedido
function calcularPedido(lineas, onSuccess, onError) {
    $.ajax({
        url: '/api/pedidos/calcular',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(lineas),
        dataType: 'json',
        success: function (data) {
            onSuccess(data);
        },
        error: function (xhr) {
            if (onError) onError(xhr);
        }
    });
}

// Enviar pedido para persistir
function crearPedido(clienteId, lineas, onSuccess, onError) {
    var payload = {
        clienteId: clienteId,
        lineas: lineas
    };

    $.ajax({
        url: '/api/pedidos',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(payload),
        dataType: 'json',
        success: function (data) {
            onSuccess(data);
        },
        error: function (xhr) {
            if (onError) onError(xhr);
        }
    });
}