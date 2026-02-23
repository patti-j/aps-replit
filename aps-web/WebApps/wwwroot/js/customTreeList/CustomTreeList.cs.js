export function attachExpandButtonHandler(dotNetRef) {
    const buttons = document.querySelectorAll('.dxbl-grid-tree-node-expand-controls-container button');
    buttons.forEach((btn, idx) => {
        btn.removeEventListener('click', btn._customExpandHandler);
        btn._customExpandHandler = function (e) {
            dotNetRef.invokeMethodAsync('OnExpandedChangedByUser', idx);
        };
        btn.addEventListener('click', btn._customExpandHandler);
    });
}
