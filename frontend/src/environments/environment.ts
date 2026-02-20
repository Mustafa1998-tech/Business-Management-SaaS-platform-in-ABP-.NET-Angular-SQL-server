import { Environment } from '@abp/ng.core';

export const environment = {
  apis: {
    default: {
      url: 'http://localhost:5069'
    }
  },
  application: {
    name: 'SaaS System',
    baseUrl: 'http://localhost:4200'
  },
  oAuthConfig: {
    issuer: 'http://localhost:5069/',
    redirectUri: 'http://localhost:4200',
    clientId: 'SaasSystem_Angular',
    responseType: 'token',
    scope: 'offline_access SaasSystem',
    oidc: false,
    requireHttps: false,
    strictDiscoveryDocumentValidation: false
  },
  localization: {
    defaultResourceName: 'SaasSystem'
  },
  production: false,
  apiBaseUrl: 'http://localhost:5069'
} as Environment & { apiBaseUrl: string };
