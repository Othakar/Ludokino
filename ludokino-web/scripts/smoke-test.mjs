const apiUrl = process.env.API_URL ?? "http://127.0.0.1:5000";
const webUrl = process.env.WEB_URL ?? "http://127.0.0.1:3000";

async function getJson(url) {
  const response = await fetch(url);
  if (!response.ok) throw new Error(`${url} returned HTTP ${response.status}`);
  return response.json();
}

async function getPage(url) {
  const response = await fetch(url);
  if (!response.ok) throw new Error(`${url} returned HTTP ${response.status}`);
  return response.text();
}

const emissions = await getJson(`${apiUrl}/api/Emissions`);
if (!Array.isArray(emissions) || emissions.length === 0) {
  throw new Error("The emissions API returned no emissions.");
}

const emissionNames = new Set(emissions.map((emission) => emission.name));
if (emissionNames.has("Critique Flashback")) {
  throw new Error("The obsolete Critique Flashback playlist is still exposed.");
}

for (const expectedName of ["Critique Contemporaine", "Critiques", "Dossier", "TOONFLASH"]) {
  const emission = emissions.find((item) => item.name === expectedName);
  if (!emission) throw new Error(`Expected playlist ${expectedName} is missing.`);
  if (!emission.imageUrl) throw new Error(`Playlist ${expectedName} has no persisted thumbnail.`);
}

const articles = await getJson(`${apiUrl}/api/Articles?page=1&pageSize=30`);
if (!Array.isArray(articles)) throw new Error("The articles API returned an invalid payload.");

const articleSummary = articles.find((article) => article.slug);
const article = articleSummary
  ? await getJson(`${apiUrl}/api/Articles/${encodeURIComponent(articleSummary.slug)}`)
  : null;
if (article && (!article.content || !article.title)) {
  throw new Error(`Article ${articleSummary.slug} has no rendered content contract.`);
}

const showsPage = await getPage(`${webUrl}/shows?smoke=1`);
if (!showsPage.includes("NOS ÉMISSIONS")) {
  throw new Error("The emissions page did not render its heading.");
}

if (article) {
  const articlePage = await getPage(`${webUrl}/blog/${encodeURIComponent(article.slug)}?smoke=1`);
  if (!articlePage.includes(article.title)) {
    throw new Error(`The article page did not render ${article.slug}.`);
  }
}

const emissionWithImage = emissions.find((emission) => emission.imageUrl);
if (emissionWithImage) {
  const proxyUrl = `${webUrl}/api/image?url=${encodeURIComponent(emissionWithImage.imageUrl)}`;
  const proxyResponse = await fetch(proxyUrl);
  if (!proxyResponse.ok || !proxyResponse.headers.get("content-type")?.startsWith("image/")) {
    throw new Error(`The image proxy failed for ${emissionWithImage.imageUrl}.`);
  }
  if (!showsPage.includes("/api/image?url=")) {
    throw new Error("The emissions page does not use the local image proxy.");
  }
}

console.log(`Smoke test passed: ${emissions.length} emissions, ${articles.length} articles${article ? `, ${article.slug}` : ""}`);
