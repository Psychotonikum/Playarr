export default function getPathWithUrlBase(path: string) {
  return `${window.Playarr.urlBase}${path}`;
}
