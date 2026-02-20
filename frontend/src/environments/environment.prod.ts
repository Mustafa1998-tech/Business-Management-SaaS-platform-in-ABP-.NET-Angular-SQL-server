import { Environment } from '@abp/ng.core';

export const environment = {
  apis: {
    default: {
      url: 'https://api.saas-system.local'
    }
  },
  application: {
    name: 'SaaS System',
    baseUrl: 'https://saas-system.local'
  },
  oAuthConfig: {
    issuer: 'https://api.saas-system.local/',
    redirectUri: 'https://saas-system.local',
    clientId: 'SaasSystem_Angular',
    responseType: 'token',
    scope: 'offline_access SaasSystem',
    oidc: false,
    requireHttps: true,
    strictDiscoveryDocumentValidation: true
  },
  localization: {
    defaultResourceName: 'SaasSystem'
  },
  production: true,
  apiBaseUrl: 'https://api.saas-system.local'
} as Environment & { apiBaseUrl: string };
