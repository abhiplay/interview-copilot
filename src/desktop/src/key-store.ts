import { promises as fs } from 'node:fs';
import path from 'node:path';

/** The subset of Electron's safeStorage we use (OS keychain-backed encryption). */
export interface SecretCipher {
  isEncryptionAvailable(): boolean;
  encryptString(plainText: string): Buffer;
  decryptString(encrypted: Buffer): string;
}

const PROVIDER_PATTERN = /^[a-z0-9-]{1,32}$/;

/**
 * BYOK API keys (NFR-03). Encrypted with the OS keychain (Windows DPAPI / macOS Keychain) via
 * safeStorage; only ciphertext touches disk. Refuses to store anything if encryption is unavailable.
 */
export class KeyStore {
  constructor(
    private readonly cipher: SecretCipher,
    private readonly filePath: string,
  ) {}

  static inDirectory(cipher: SecretCipher, directory: string): KeyStore {
    return new KeyStore(cipher, path.join(directory, 'keys.json'));
  }

  async get(provider: string): Promise<string | null> {
    const entry = (await this.read())[validate(provider)];
    if (!entry || !this.cipher.isEncryptionAvailable()) {
      return null;
    }
    return this.cipher.decryptString(Buffer.from(entry, 'base64'));
  }

  async set(provider: string, key: string): Promise<void> {
    if (!this.cipher.isEncryptionAvailable()) {
      throw new Error("Your OS keychain isn't available, so the key wasn't saved.");
    }
    if (typeof key !== 'string' || key.trim() === '') {
      throw new Error('The API key is empty.');
    }
    const entries = await this.read();
    entries[validate(provider)] = this.cipher.encryptString(key.trim()).toString('base64');
    await this.write(entries);
  }

  async delete(provider: string): Promise<void> {
    const entries = await this.read();
    delete entries[validate(provider)];
    await this.write(entries);
  }

  private async read(): Promise<Record<string, string>> {
    try {
      const parsed: unknown = JSON.parse(await fs.readFile(this.filePath, 'utf8'));
      return typeof parsed === 'object' && parsed !== null ? (parsed as Record<string, string>) : {};
    } catch {
      return {};
    }
  }

  private async write(entries: Record<string, string>): Promise<void> {
    await fs.mkdir(path.dirname(this.filePath), { recursive: true });
    await fs.writeFile(this.filePath, JSON.stringify(entries), { encoding: 'utf8', mode: 0o600 });
  }
}

function validate(provider: string): string {
  if (typeof provider !== 'string' || !PROVIDER_PATTERN.test(provider)) {
    throw new Error('Unknown provider.');
  }
  return provider;
}
