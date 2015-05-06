﻿using System;
using System.Collections.Generic;

namespace SimpleScene
{
	public class SSSkeletalJointRuntime
	{
		public List<int> Children = new List<int>();
		public SSSkeletalJointLocation CurrentLocation;

		protected SSSkeletalJoint m_baseInfo;

		public SSSkeletalJoint BaseInfo {
			get { return m_baseInfo; }
		}

		public SSSkeletalJointRuntime(SSSkeletalJoint baseInfo)
		{
			m_baseInfo = baseInfo;
			CurrentLocation = m_baseInfo.BaseLocation;
		}
	}

	public class SSSkeletalHierarchyRuntime
	{
		protected readonly SSSkeletalJointRuntime[] m_joints = null;
		protected readonly List<int> m_topLevelJoints = new List<int> ();

		public SSSkeletalJointRuntime[] Joints {
			get { return m_joints; }
		}

		public int NumJoints {
			get { return m_joints.Length; }
		}

		public SSSkeletalHierarchyRuntime(SSSkeletalJoint[] joints)
		{
			m_joints = new SSSkeletalJointRuntime[joints.Length];
			for (int j = 0; j < joints.Length; ++j) {
				var jointInput = joints [j];
				m_joints [j] = new SSSkeletalJointRuntime(jointInput);
				int parentIdx = jointInput.ParentIndex;
				if (parentIdx < 0) {
					m_topLevelJoints.Add (j);
				} else {
					m_joints [parentIdx].Children.Add (j);
				}
			}
		}

		public int JointIndex(string jointName)
		{
			if (String.Compare (jointName, "all", true) == 0) {
				return -1;
			}

			for (int j = 0; j < m_joints.Length; ++j) {
				if (m_joints [j].BaseInfo.Name == jointName) {
					return j;
				}
			}
			string errMsg = string.Format ("Joint not found: \"{0}\"", jointName);
			System.Console.WriteLine (errMsg);
			throw new Exception (errMsg);
		}

		public SSSkeletalJointLocation JointLocation(int jointIdx) 
		{
			return m_joints [jointIdx].CurrentLocation;
		}

		public int[] TopLevelJoints {
			get { return m_topLevelJoints.ToArray (); }
		}

		public void VerifyAnimation(SSSkeletalAnimation animation)
		{
			if (this.NumJoints != animation.NumJoints) {
				string str = string.Format (
					"Joint number mismatch: {0} in md5mesh, {1} in md5anim",
					this.NumJoints, animation.NumJoints);
				Console.WriteLine (str);
				throw new Exception (str);
			}
			for (int j = 0; j < NumJoints; ++j) {
				SSSkeletalJoint meshInfo = this.m_joints [j].BaseInfo;
				SSSkeletalJoint animInfo = animation.JointHierarchy [j];
				if (meshInfo.Name != animInfo.Name) {
					string str = string.Format (
						"Joint name mismatch: {0} in md5mesh, {1} in md5anim",
						meshInfo.Name, animInfo.Name);
					Console.WriteLine (str);
					throw new Exception (str);
				}
				if (meshInfo.ParentIndex != animInfo.ParentIndex) {
					string str = string.Format (
						"Hierarchy parent mismatch for joint \"{0}\": {1} in md5mesh, {2} in md5anim",
						meshInfo.Name, meshInfo.ParentIndex, animInfo.ParentIndex);
					Console.WriteLine (str);
					throw new Exception (str);
				}
			}
		}

		public void LoadAnimationFrame(SSSkeletalAnimation anim, float t)
		{
			for (int j = 0; j < NumJoints; ++j) {
				m_joints [j].CurrentLocation = anim.ComputeJointFrame (j, t);
			}
		}

		public void ApplyAnimationChannels(List<SSSkeletalAnimationChannelRuntime> channels)
		{
			foreach (int j in m_topLevelJoints) {
				traverseWithChannels (j, channels, null, null);
			}
		}

		private void traverseWithChannels(int jointIdx, 
			List<SSSkeletalAnimationChannelRuntime> channels,
			SSSkeletalAnimationChannelRuntime activeChannel,
			SSSkeletalAnimationChannelRuntime prevActiveChannel)
		{
			foreach (var channel in channels) {
				if (channel.IsActive && channel.TopLevelActiveJoints.Contains (jointIdx)) {
					if (activeChannel != null && !channel.IsEnding) {
						prevActiveChannel = activeChannel;
					}
					activeChannel = channel;
				}
			}
			SSSkeletalJointRuntime joint = m_joints [jointIdx];

			if (activeChannel == null) {
				joint.CurrentLocation = joint.BaseInfo.BaseLocation;
			} else {
				SSSkeletalJointLocation activeLoc = activeChannel.ComputeJointFrame (jointIdx);
				int parentIdx = joint.BaseInfo.ParentIndex;
				if (parentIdx != -1) {
					activeLoc.ApplyParentTransform (m_joints [parentIdx].CurrentLocation);
				}

				if (activeChannel.IsEnding) {
					// TODO smarter, multi layer fallback
					SSSkeletalJointLocation fallbackLoc;
					if (prevActiveChannel == null || prevActiveChannel.IsEnding) {
						fallbackLoc = joint.BaseInfo.BaseLocation;
					} else {
						fallbackLoc = prevActiveChannel.ComputeJointFrame (jointIdx);
						if (joint.BaseInfo.ParentIndex != -1) {
							fallbackLoc.ApplyParentTransform (m_joints [parentIdx].CurrentLocation);
						}
					}
					joint.CurrentLocation = SSSkeletalJointLocation.Interpolate (
						activeLoc, fallbackLoc, activeChannel.FadeBlendPosition);
				} else {
					joint.CurrentLocation = activeLoc;
				}
			}

			foreach (int child in joint.Children) {
				traverseWithChannels (child, channels, activeChannel, prevActiveChannel);
			}
		}
	}
}
