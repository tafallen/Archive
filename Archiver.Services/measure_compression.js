const http = require('http');

function makeRequest(path, headers = {}) {
  return new Promise((resolve, reject) => {
    const req = http.request({
      hostname: 'localhost',
      port: 5112,
      path: path,
      method: 'GET',
      headers: {
        'X-Internal-Key': 'REPLACE_ME_WITH_REAL_SECRET',
        ...headers
      }
    }, (res) => {
      let data = [];
      res.on('data', chunk => data.push(chunk));
      res.on('end', () => {
        const buffer = Buffer.concat(data);
        resolve({
          statusCode: res.statusCode,
          headers: res.headers,
          size: buffer.length,
          contentEncoding: res.headers['content-encoding'] || 'none'
        });
      });
    });
    req.on('error', reject);
    req.end();
  });
}

async function run() {
  try {
    const uncompressed = await makeRequest('/weatherforecast');
    console.log(`Uncompressed: ${uncompressed.size} bytes, Encoding: ${uncompressed.contentEncoding}`);

    const gzipped = await makeRequest('/weatherforecast', { 'Accept-Encoding': 'gzip' });
    console.log(`Requested gzip: ${gzipped.size} bytes, Encoding: ${gzipped.contentEncoding}`);

    const brotli = await makeRequest('/weatherforecast', { 'Accept-Encoding': 'br' });
    console.log(`Requested brotli: ${brotli.size} bytes, Encoding: ${brotli.contentEncoding}`);
  } catch (err) {
    console.error(err);
  }
}

run();
