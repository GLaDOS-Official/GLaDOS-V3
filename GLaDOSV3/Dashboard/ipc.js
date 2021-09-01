const net = require('net');
var server = null;
var client = null;
function begin() {
	server = net.createServer((c) => {
		console.log('GLaDOS connected!');
		client = c;
		c.on('end', () => {
			console.log('GLaDOS disconnected :(');
		});
		client.on('data', (data) => {
			console.log(readString(data));
		});
		writeString("Hello!");
	});
	server.listen('\\\\?\\pipe\\GLaDOS_Dashboard');
}
function readString(data) {
    var len;
	var c = data.toString();
    len =  c.charCodeAt(0) * 256;
    len += c.charCodeAt(1);
	console.log(len);
	var str = '';
	for(var i = 2; i < len + 2; i++) {
		str += c[i];
	}
	return str;
}
function writeString(string) {
	var buffer = Buffer.from(string, 'utf-8');
	var len = buffer.length;
	client.write(String.fromCharCode((len / 256)));
	client.write(String.fromCharCode((len & 255)));
	client.write(buffer);
}

module.exports = {
  begin: begin,
  writeString: writeString
};