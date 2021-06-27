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
	});
	server.listen('\\\\?\\pipe\\GLaDOS_Dashboard');
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