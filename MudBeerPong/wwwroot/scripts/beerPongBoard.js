window.beerPongBoard = {
    startDrag: function (dotNetRef) {
        function moveHandler(e) {
            let x = e.touches ? e.touches[0].clientX : e.clientX;
            let y = e.touches ? e.touches[0].clientY : e.clientY;
            dotNetRef.invokeMethodAsync('OnPointerMove', x, y);
        }
        function upHandler() {
            document.removeEventListener('pointermove', moveHandler);
            document.removeEventListener('pointerup', upHandler);
            document.removeEventListener('touchmove', moveHandler);
            document.removeEventListener('touchend', upHandler);
        }
        document.addEventListener('pointermove', moveHandler);
        document.addEventListener('pointerup', upHandler);
        document.addEventListener('touchmove', moveHandler);
        document.addEventListener('touchend', upHandler);
    }
};