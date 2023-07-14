var ImageUploaderPlugin = {
  ImageUploaderCaptureClick: function() {
    if (!document.getElementById('ImageUploaderInput')) {
      var fileInput = document.createElement('input');
      fileInput.setAttribute('type', 'file');
      fileInput.setAttribute('id', 'ImageUploaderInput');
      fileInput.style.visibility = 'hidden';
      fileInput.onclick = function (event) {
        this.value = null;
      };
      fileInput.onchange = function (event) {
        SendMessage('Canvas', 'FileSelected', URL.createObjectURL(event.target.files[0]));
      }
      document.body.appendChild(fileInput);
    }
    var OpenFileDialog = function() {
      document.getElementById('ImageUploaderInput').click();
      document.getElementById('canvas').removeEventListener('click', OpenFileDialog);
    };
    document.getElementById('canvas').addEventListener('click', OpenFileDialog, false);
  }
};
mergeInto(LibraryManager.library, ImageUploaderPlugin);