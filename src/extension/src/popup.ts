import { renderStatus } from './popup/render';
import type { Request, StatusResponse } from './shared/messages';

const send = (request: Request) => chrome.runtime.sendMessage(request) as Promise<StatusResponse>;

document.getElementById('sign-in')!.addEventListener('submit', (event) => {
  event.preventDefault();
  const username = (document.getElementById('username') as HTMLInputElement).value;
  const password = (document.getElementById('password') as HTMLInputElement).value;
  void send({ kind: 'sign-in', username, password }).then((status) => renderStatus(document, status));
});

document.getElementById('sign-out')!.addEventListener('click', () => {
  void send({ kind: 'sign-out' }).then((status) => renderStatus(document, status));
});

void send({ kind: 'status' }).then((status) => renderStatus(document, status));
