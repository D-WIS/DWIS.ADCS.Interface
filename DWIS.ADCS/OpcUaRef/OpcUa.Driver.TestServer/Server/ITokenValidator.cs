using Opc.Ua;

namespace Slb.OpcUa.Driver.TestServer;

public interface ITokenValidator
{
	IUserIdentity ValidateToken(IssuedIdentityToken issuedToken);
}