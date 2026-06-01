const response = await fetch("http://127.0.0.1:3000");

if (!response.ok) {
  process.exit(1);
}
