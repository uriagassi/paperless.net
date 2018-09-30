window.onload = function(e) {
	var list = document.getElementsByClassName('paperless-attachment-file');

for (var i = 0; i < list.length; i++) {
  var src = list[i].getAttribute('data-ext');
  list[i].style.backgroundImage="url('images/" + src.substring(1) + "-icon-48x48.png')";
}
}