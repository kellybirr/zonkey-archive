using System.ComponentModel;

namespace Zonkey.ObjectModel
{
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly")]
    public abstract class DataComponent : DataClass, IComponent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Zonkey.ObjectModel.DataComponent"/> class.
        /// </summary>
        /// <param name="addingNew">if set to <c>true</c> then initializes object for insertion to database.</param>
        protected DataComponent(bool addingNew) : base(addingNew)
        {
        }

        #region IComponent Members

        /// <summary>
        /// Gets or sets the <see cref="T:System.ComponentModel.ISite"></see> associated with the <see cref="T:System.ComponentModel.IComponent"></see>.
        /// </summary>
        /// <value></value>
        /// <returns>The <see cref="T:System.ComponentModel.ISite"></see> object associated with the component; or null, if the component does not have a site.</returns>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        ISite IComponent.Site
        {
            get { return Site; }
            set { Site = value; }
        }

        /// <summary>
        /// Gets or sets the <see cref="T:System.ComponentModel.ISite"></see> associated with the <see cref="T:System.ComponentModel.IComponent"></see>.
        /// </summary>
        /// <value></value>
        /// <returns>The <see cref="T:System.ComponentModel.ISite"></see> object associated with the component; or null, if the component does not have a site.</returns>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        protected virtual ISite Site
        {
            get { return _site; }
            set { _site = value; }
        }

        private ISite _site;

        #endregion
    }
}