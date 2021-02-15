//function copyToClipboard(id) {
//    console.log('selected id:' + id);
//    var text = document.getElementById(id);
//    text.select();
//    document.execCommand('copy');
//}

function copyToClipboard(text) {
    console.log(text);
    var textarea = document.createElement('textarea');
    textarea.value = text;
    document.body.appendChild(textarea);
    textarea.select();
    document.execCommand('copy');
    document.body.removeChild(textarea);
}