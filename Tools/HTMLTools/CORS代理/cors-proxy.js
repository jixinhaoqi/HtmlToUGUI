//请确保已安装node环境

const http = require('http');
const https = require('https');
const { URL } = require('url');

const PORT = 8888;
const MAX_REDIRECTS = 5;

function doRequest(urlStr, redirectCount = 0) {
    return new Promise((resolve, reject) => {
        if (redirectCount > MAX_REDIRECTS) {
            return reject(new Error('重定向次数过多'));
        }

        const parsedUrl = new URL(urlStr);
        const client = parsedUrl.protocol === 'https:' ? https : http;

        const options = {
            hostname: parsedUrl.hostname,
            port: parsedUrl.port,
            path: parsedUrl.pathname + parsedUrl.search,
            method: 'GET',
            headers: {
                'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36',
                'Accept': '*/*',
                'Accept-Language': 'zh-CN,zh;q=0.9',
            }
        };

        client.get(options, (res) => {
            // 处理重定向
            if (res.statusCode >= 300 && res.statusCode < 400 && res.headers.location) {
                const redirectUrl = new URL(res.headers.location, urlStr).href;
                console.log(`跟随重定向: ${redirectUrl}`);
                resolve(doRequest(redirectUrl, redirectCount + 1));
                return;
            }

            let data = [];
            res.on('data', (chunk) => data.push(chunk));
            res.on('end', () => {
                const body = Buffer.concat(data);
                resolve({
                    statusCode: res.statusCode,
                    headers: res.headers,
                    body: body.toString()
                });
            });
        }).on('error', reject);
    });
}

const server = http.createServer(async (req, res) => {
    // 设置 CORS
    res.setHeader('Access-Control-Allow-Origin', '*');
    res.setHeader('Access-Control-Allow-Methods', 'GET');

    const targetUrl = decodeURIComponent(req.url.slice(1));
    if (!targetUrl) {
        res.writeHead(400);
        res.end('请在路径中提供要抓取的网址');
        return;
    }

    console.log(`正在获取: ${targetUrl}`);

    try {
        const result = await doRequest(targetUrl);
        console.log(`获取成功，数据长度: ${result.body.length}`);
        res.writeHead(result.statusCode, {
            'Content-Type': result.headers['content-type'] || 'text/plain'
        });
        res.end(result.body);
    } catch (err) {
        console.error(`请求失败: ${err.message}`);
        res.writeHead(500);
        res.end('代理请求失败');
    }
});

server.listen(PORT, () => {
    console.log(`✅ CORS 代理已启动：http://localhost:${PORT}`);
	console.log(`   两次Ctrl+C终止代理`);
    console.log(`   把http://localhost:${PORT}/复制粘贴到HTML解构工具自定义CORS代理输入框即可`);
});